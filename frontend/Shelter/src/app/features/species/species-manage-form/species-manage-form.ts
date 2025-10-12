import { CommonModule } from '@angular/common';
import { Component, inject, OnInit } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { RouterLink } from '@angular/router';
import { SpeciesService } from '../../../core/services/species.service';
import { SpeciesDto, CreateSpeciesDto } from '../../../core/models/species';

type RowState = {
  editing: boolean;
  // edytowane pola:
  tempName: string;
  tempRequiresPermit: boolean;
  tempPermitName: string;
  tempPermitAuthority: string;
  tempPermitNotes: string;

  // usuwanie:
  confirmingDelete: boolean;
  confirmText: string;
  showDetails: boolean;
};

@Component({
  selector: 'app-species-manage-form',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './species-manage-form.html',
  styleUrls: ['./species-manage-form.css']
})
export class SpeciesManageForm implements OnInit {
  private speciesService = inject(SpeciesService);

  loading = true;
  savingId: number | null = null;
  deletingId: number | null = null;
  error = '';

  species: SpeciesDto[] = [];
  state = new Map<number, RowState>();

  ngOnInit(): void {
    this.load();
  }

  load() {
    this.loading = true;
    this.error = '';
    this.speciesService.getAllSpecies().subscribe({
      next: list => {
        this.species = list;
        this.state.clear();
        for (const s of list) {
          this.state.set(s.id, {
            editing: false,
            tempName: s.name ?? '',
            tempRequiresPermit: !!s.requiresPermit,
            tempPermitName: s.permitName ?? '',
            tempPermitAuthority: s.permitAuthority ?? '',
            tempPermitNotes: s.permitNotes ?? '',
            confirmingDelete: false,
            confirmText: '',
            showDetails: false
          });
        }
        this.loading = false;
      },
      error: () => {
        this.error = 'Nie udało się pobrać listy gatunków.';
        this.loading = false;
      }
    });
  }

  startEdit(s: SpeciesDto) {
    const st = this.state.get(s.id);
    if (!st) return;
    st.tempName = s.name ?? '';
    st.tempRequiresPermit = !!s.requiresPermit;
    st.tempPermitName = s.permitName ?? '';
    st.tempPermitAuthority = s.permitAuthority ?? '';
    st.tempPermitNotes = s.permitNotes ?? '';
    st.editing = true;
  }

  cancelEdit(s: SpeciesDto) {
    const st = this.state.get(s.id);
    if (!st) return;
    st.tempName = s.name ?? '';
    st.tempRequiresPermit = !!s.requiresPermit;
    st.tempPermitName = s.permitName ?? '';
    st.tempPermitAuthority = s.permitAuthority ?? '';
    st.tempPermitNotes = s.permitNotes ?? '';
    st.editing = false;
  }

  private buildDtoFromState(st: RowState): CreateSpeciesDto {
    const requires = !!st.tempRequiresPermit;
    return {
      name: (st.tempName || '').trim(),
      requiresPermit: requires,
      permitName: requires ? ((st.tempPermitName || '').trim() || null) : null,
      permitAuthority: requires ? ((st.tempPermitAuthority || '').trim() || null) : null,
      permitNotes: ((st.tempPermitNotes || '').trim() || null)
    };
  }

  private isValidState(st: RowState): boolean {
    const nameOk = (st.tempName || '').trim().length >= 2;
    if (!st.tempRequiresPermit) return nameOk;
    const pnOk = (st.tempPermitName || '').trim().length > 0;
    const paOk = (st.tempPermitAuthority || '').trim().length > 0;
    return nameOk && pnOk && paOk;
  }

  save(s: SpeciesDto) {
    const st = this.state.get(s.id);
    if (!st) return;

    // walidacja
    if (!this.isValidState(st)) {
      alert('Uzupełnij wymagane pola (nazwa, a przy pozwoleniu: nazwa pozwolenia i organ).');
      return;
    }

    const dto = this.buildDtoFromState(st);

    // czy coś się faktycznie zmieniło?
    const noChanges =
      dto.name === (s.name || '').trim() &&
      dto.requiresPermit === !!s.requiresPermit &&
      (dto.permitName || null) === (s.permitName || null) &&
      (dto.permitAuthority || null) === (s.permitAuthority || null) &&
      (dto.permitNotes || null) === (s.permitNotes || null);

    if (noChanges) {
      st.editing = false;
      return;
    }

    this.savingId = s.id;
    this.speciesService.editSpecies(s.id, dto).subscribe({
      next: () => {
        // optymistyczna aktualizacja UI
        s.name = dto.name;
        s.requiresPermit = dto.requiresPermit;
        s.permitName = dto.permitName ?? undefined;
        s.permitAuthority = dto.permitAuthority ?? undefined;
        s.permitNotes = dto.permitNotes ?? undefined;

        st.editing = false;
        this.savingId = null;
      },
      error: (err) => {
        this.savingId = null;
        const msg = err?.error?.message || 'Nie udało się zapisać zmian gatunku.';
        alert(msg);
      }
    });
  }

  openDelete(s: SpeciesDto) {
    const st = this.state.get(s.id);
    if (!st) return;
    st.confirmingDelete = true;
    st.confirmText = '';
  }

  cancelDelete(s: SpeciesDto) {
    const st = this.state.get(s.id);
    if (!st) return;
    st.confirmingDelete = false;
    st.confirmText = '';
  }

  confirmDelete(s: SpeciesDto) {
    const st = this.state.get(s.id);
    if (!st || st.confirmText !== s.name) return;

    this.deletingId = s.id;
    this.speciesService.deleteSpecies(s.id).subscribe({
      next: () => {
        this.species = this.species.filter(x => x.id !== s.id);
        this.state.delete(s.id);
        this.deletingId = null;
      },
      error: (err) => {
        this.deletingId = null;
        const msg = err?.error?.message || 'Nie udało się usunąć gatunku.';
        alert(msg);
      }
    });
  }
}
