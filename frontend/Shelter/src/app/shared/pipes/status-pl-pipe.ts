import { Pipe, PipeTransform } from '@angular/core';

@Pipe({
  name: 'statusPl'
})
export class StatusPlPipe implements PipeTransform {

  transform(value?: string | null): string {
    switch ((value || '').toLowerCase()) {
      case 'available': return 'Dostępny';
      case 'reserved': return 'Zarezerwowany';
      case 'adopted': return 'Adoptowany';
      case 'notavailable': return 'Niedostępny';
      default: return '—';
    }
  }

}
