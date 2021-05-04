import { Injectable, EventEmitter } from '@angular/core';    
import { Subscription } from 'rxjs/internal/Subscription';    
    
@Injectable({    
  providedIn: 'root'    
})    
export class EventEmitterService {    
    
  invokeOpenColumnsSettingsFunction = new EventEmitter();  
  subsVarColumnsSettings: Subscription; 

  invokeOpenImpersonationSettingsFunction = new EventEmitter();  
  subsVarImpersonationSettings: Subscription; 

  invokeCreateNewLetterFunction = new EventEmitter();  
  subsCreateNewLetter: Subscription; 
   
  invokeIncomingDashboardFunction = new EventEmitter();  
  subsIncomingDashboard: Subscription; 
    
  constructor() { }    
    
  onOpenColumnsSettingsClick() {    
    this.invokeOpenColumnsSettingsFunction.emit();    
  }    
    
  onOpenImpersonationSettingsClick() {    
    this.invokeOpenImpersonationSettingsFunction.emit();    
  }     
    
  onCreateNewLetterClick() {    
    this.invokeCreateNewLetterFunction.emit();    
  }       
    
  onIncomingDashboard(inCommingDashboard: any) {    
    this.invokeIncomingDashboardFunction.emit(inCommingDashboard);    
  }   
}   