import { Pipe, PipeTransform } from '@angular/core';

@Pipe({
  name: 'sexPl'
})
export class SexPlPipe implements PipeTransform {

  transform(value?: string | null): string {
    switch((value || '').toLowerCase()){
      case 'female': return 'Samica'
      case 'male': return 'Samiec'
      case 'unknow': return 'Nieznana'
      default: return 'Nieznana'
    }
  }

}
