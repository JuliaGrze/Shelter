import {
  Component,
  ElementRef,
  inject,
  OnInit,
  signal,
  ViewChild
} from '@angular/core';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { AdoptionService } from '../../../core/services/adoption.service';
import { AdoptionApplicationDto, AdoptionDetailsDto } from '../../../core/models/adoption';
import { CommonModule, DatePipe } from '@angular/common';
import { enviroment } from '../../../../environments/environment';

@Component({
  selector: 'app-details-application-adoption',
  imports: [
    CommonModule,
    RouterLink,
    DatePipe
  ],
  templateUrl: './details-application-adoption.html',
  styleUrl: './details-application-adoption.css'
})
export class DetailsApplicationAdoption implements OnInit {
  private route = inject(ActivatedRoute);
  private router = inject(Router);
  private adoptionService = inject(AdoptionService);

  loading = signal(true);
  error = signal<string | null>(null);
  app = signal<AdoptionDetailsDto | null>(null);

  // baza backendu do składania linku do PDF
  backendUrl = enviroment.backendUrl;

  // --- PODPIS (canvas) ---
  @ViewChild('sigCanvas') sigCanvas?: ElementRef<HTMLCanvasElement>;

  showSignature = signal(false);
  signLoading = signal(false);
  signError = signal<string | null>(null);

  private isDrawing = false;
  private lastX = 0;
  private lastY = 0;

  ngOnInit(): void {
    const idParam = this.route.snapshot.paramMap.get('id');
    const id = idParam ? Number(idParam) : NaN;

    this.load(id);
  }

  private load(id: number) {
    this.loading.set(true);
    this.error.set(null);

    this.adoptionService.get(id).subscribe({
      next: a => {
        this.app.set(a);
        this.loading.set(false);
      },
      error: err => {
        console.error(err);
        this.error.set('Nie udało się załadować wniosku adopcyjnego.');
        this.loading.set(false);
      }
    });
  }

  goBack() {
    this.router.navigate(['/adoption/my/applications']);
  }

  statusClass(code: string): string {
    switch (code) {
      case 'Submitted':          return 'badge--submitted';
      case 'InReview':           return 'badge--inreview';
      case 'HomeVisitScheduled': return 'badge--visit-scheduled';
      case 'HomeVisitCompleted': return 'badge--visit-completed';
      case 'Approved':           return 'badge--approved';
      case 'Rejected':           return 'badge--rejected';
      case 'Withdrawn':          return 'badge--withdrawn';
      case 'ContractSigned':     return 'badge--contract-signed';
      case 'ContractGenerated':  return 'badge--contract-generated';
      default:                   return 'badge--neutral';
    }
  }

  // ====== PODPIS – logika canvas ======

  openSignature() {
    this.signError.set(null);
    this.showSignature.set(true);

    // po otwarciu wyczyść canvas
    setTimeout(() => this.clearSignature(), 0);
  }

  closeSignature() {
    this.showSignature.set(false);
  }

  private getCanvas(): HTMLCanvasElement | null {
    return this.sigCanvas?.nativeElement ?? null;
  }

  startDraw(event: MouseEvent) {
    const canvas = this.getCanvas();
    if (!canvas) return;

    this.isDrawing = true;
    const rect = canvas.getBoundingClientRect();
    this.lastX = event.clientX - rect.left;
    this.lastY = event.clientY - rect.top;
  }

  draw(event: MouseEvent) {
    if (!this.isDrawing) return;

    const canvas = this.getCanvas();
    if (!canvas) return;

    const ctx = canvas.getContext('2d');
    if (!ctx) return;

    const rect = canvas.getBoundingClientRect();
    const x = event.clientX - rect.left;
    const y = event.clientY - rect.top;

    ctx.lineWidth = 2;
    ctx.lineCap = 'round';
    ctx.strokeStyle = '#000';

    ctx.beginPath();
    ctx.moveTo(this.lastX, this.lastY);
    ctx.lineTo(x, y);
    ctx.stroke();

    this.lastX = x;
    this.lastY = y;
  }

  stopDraw() {
    this.isDrawing = false;
  }

  clearSignature() {
    const canvas = this.getCanvas();
    if (!canvas) return;

    const ctx = canvas.getContext('2d');
    if (!ctx) return;

    ctx.clearRect(0, 0, canvas.width, canvas.height);
  }

  confirmSignature() {
    const current = this.app();
    if (!current) return;

    const canvas = this.getCanvas();
    if (!canvas) {
      this.signError.set('Brak pola podpisu.');
      return;
    }

    const dataUrl = canvas.toDataURL('image/png');

    if (dataUrl.length < 100) {
      this.signError.set('Podpis wygląda na pusty – narysuj swój podpis.');
      return;
    }

    this.signLoading.set(true);
    this.signError.set(null);

    this.adoptionService.signContract(current.id, dataUrl).subscribe({
      next: () => {
        this.signLoading.set(false);
        this.showSignature.set(false);
        this.load(current.id); // odśwież dane (status/umowa)
      },
      error: () => {
        this.signLoading.set(false);
        this.signError.set('Nie udało się podpisać umowy. Spróbuj ponownie.');
      }
    });
  }
}
