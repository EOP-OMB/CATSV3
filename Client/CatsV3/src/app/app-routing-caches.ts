import {
    RouteReuseStrategy,
    DefaultUrlSerializer,
    ActivatedRouteSnapshot,
    DetachedRouteHandle
  } from '@angular/router';
  
  export class AppRoutingCache implements RouteReuseStrategy {
    public static handlers: { [key: string]: DetachedRouteHandle } = {};
  
    
    public shouldDetach(route: ActivatedRouteSnapshot): boolean {
      
      return true;
    }
  
    
    public store(
      route: ActivatedRouteSnapshot,
      handle: DetachedRouteHandle
    ): void {
     
      AppRoutingCache.handlers[route.routeConfig.path] = handle;
    }
  
    
    public shouldAttach(route: ActivatedRouteSnapshot): boolean {
      return (
        !!route.routeConfig && !!AppRoutingCache.handlers[route.routeConfig.path]
      );
    }
    
    public retrieve(route: ActivatedRouteSnapshot): DetachedRouteHandle {
      if (!route.routeConfig) {
        return null;
      }
      return AppRoutingCache.handlers[route.routeConfig.path];
    }
  
    
    public shouldReuseRoute(
      future: ActivatedRouteSnapshot,
      current: ActivatedRouteSnapshot
    ): boolean {
      return future.routeConfig === current.routeConfig;
    }
  }
  