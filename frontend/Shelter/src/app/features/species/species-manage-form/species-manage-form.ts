import { CommonModule } from '@angular/common';
import { Component, inject, OnInit } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { SpeciesService } from '../../../core/services/species.service';
import { SpeciesDto } from '../../../core/models/species'
import { RouterLink } from '@angular/router';

type RowState = {
  editing: boolean;
  tempName: string;
  confirmingDelete: boolean;
  confirmText: string;
};

@Component({
  selector: 'app-species-manage-form',
  imports: [
    CommonModule,
    FormsModule,
    RouterLink
],
  templateUrl: './species-manage-form.html',
  styleUrl: './species-manage-form.css'
})
export class SpeciesManageForm implements OnInit{
  private speciesService = inject(SpeciesService)

  loading = true
  savingId: number | null = null
  deletingId: number | null = null
  error = ''

  species: SpeciesDto[] = []
  state = new Map<number, RowState>()

  ngOnInit(): void {
    this.load()
  }

  load(){
    this.loading = true
    this.error = ''
    this.speciesService.getAllSpecies().subscribe({
      next: list => {
        this.species = list
        this.state.clear()
        for(const s of list){
          this.state.set(s.id, {
            editing: false,
            tempName: s.name,
            confirmingDelete: false,
            confirmText: ''
          })
        }
        this.loading = false
      },
      error: () =>{
        this.error = 'Nie udało się pobrać listy gatunków.';
        this.loading = false;
      }
    })
  }

  startEdit(s: SpeciesDto){
    const st = this.state.get(s.id)
    if(!st) return
    st.tempName = s.name
    st.editing = true
  }

  cancelEdit(s: SpeciesDto) {
    const st = this.state.get(s.id);
    if (!st) return;
    st.tempName = s.name;
    st.editing = false;
  }

  save(s: SpeciesDto) {
    const st = this.state.get(s.id);
    if (!st) return;
    const name = (st.tempName ?? '').trim();
    if (!name || name === s.name) {
      st.editing = false; // nic do zapisania
      return;
    }
    this.savingId = s.id;
    this.speciesService.editSpecies(s.id, name).subscribe({
      next: () => {
        s.name = name;       // optymistycznie aktualizujemy UI
        st.editing = false;
        this.savingId = null;
      },
      error: () => {
        this.savingId = null;
        // zostaw edycję otwartą, pokaż komunikat
        alert('Nie udało się zapisać zmian gatunku.');
      },
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
      error: () => {
        this.deletingId = null;
        alert('Nie udało się usunąć gatunku.');
      },
    });
  }

}
