import { RouteReuseStrategy, ActivatedRouteSnapshot, DetachedRouteHandle } from '@angular/router';
import { clearLocalStorage, clearSessionStorage } from './modules/shared/utilities/utility-functions';
import { Injectable } from "@angular/core";

@Injectable()
export class CustomRouteReuseStrategy implements RouteReuseStrategy {

    private storedRoutes = new Map<string, DetachedRouteHandle>();
    
    shouldDetach(route: ActivatedRouteSnapshot): boolean {
        return false;
    }
    
    store(route: ActivatedRouteSnapshot, detachedTree: DetachedRouteHandle): void {

    }
    
    shouldAttach(route: ActivatedRouteSnapshot): boolean {
        return false;
    }
    
    retrieve(route: ActivatedRouteSnapshot): DetachedRouteHandle|null {
        return null;
    }
    
    shouldReuseRoute(curr: ActivatedRouteSnapshot, future: ActivatedRouteSnapshot): boolean {
        if (future.routeConfig != null && curr.routeConfig != null){
            //if (future.routeConfig.path == 'cdetails' && curr.routeConfig.path == 'cdetails'){
            if (future.routeConfig.path == curr.routeConfig.path){
                return false;
            }
            else{
                // clearSessionStorage('cATSSelectedDashboardOptions');
                // clearSessionStorage('surrogateUser');
                return future.routeConfig === curr.routeConfig;
            }  
        }
        else{
            return future.routeConfig === curr.routeConfig;
        }
              
    }
}