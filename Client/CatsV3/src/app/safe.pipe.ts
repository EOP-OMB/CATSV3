import { Pipe, PipeTransform } from '@angular/core';
import {DomSanitizer, SafeHtml,SafeStyle,SafeScript,SafeUrl,SafeResourceUrl} from '@angular/platform-browser'

@Pipe({
  name: 'safe'
})
export class SafePipe implements PipeTransform {

  constructor(protected sanitizer: DomSanitizer){}

  transform(value: any, type: string): DomSanitizer |  SafeHtml | SafeStyle | SafeScript | SafeUrl | SafeResourceUrl {
    
    switch (type) {
      case 'html': return this.sanitizer.bypassSecurityTrustHtml(value);
      case 'style': return this.sanitizer.bypassSecurityTrustHtml(value);
      case 'script': return this.sanitizer.bypassSecurityTrustHtml(value);
      case 'url': return this.sanitizer.bypassSecurityTrustHtml(value);
      case 'resourceUrl': return this.sanitizer.bypassSecurityTrustHtml(value);    
      default: throw new Error('Invalid safe type specified: ${type}');
    }
    
    
    return null;
  }

}
