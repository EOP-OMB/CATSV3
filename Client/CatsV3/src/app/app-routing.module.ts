import { NgModule } from '@angular/core';
import { Routes, RouterModule, ExtraOptions, Router, RouteReuseStrategy } from '@angular/router';
import { ForbiddenComponent } from './modules/shared/components/forbidden/forbidden.component';
import { CustomRouteReuseStrategy } from './app-route-CustomRouteReuseStrategy';

const routes1: Routes = [];

const routes: Routes = [
    { path: '', redirectTo: 'home', pathMatch: 'full' },
    {
        path: 'home',
        loadChildren: () =>
            import('src/app/modules/home/home.module').then(m => m.HomeModule)
    },
    {
        path: 'correspondence',
        loadChildren: () =>
            import('src/app/modules/correspondence/correspondence.module').then(m => m.CorrespondenceModule),
            data: {
                title:"Correspondence Dashboard"
            }
    },
    {
        path: 'originator',
        loadChildren: () =>
            import('src/app/modules/originator/originator.module').then(m => m.OriginatorModule),
            data: {
                title:"Originator Dashboard"
            }
    },
    {
        path: 'reviewer',
        loadChildren: () =>
            import('src/app/modules/reviewer/reviewer.module').then(m => m.ReviewerModule),
            data: {
                title:"Reviewer Dashboard"
            }
    },
    {
        path: 'report',
        loadChildren: () =>
            import('src/app/modules/report/report.module').then(m => m.ReportModule),
            data: {
                title:"Reports"
            }
    },
    {
        path: 'admin',
        loadChildren: () =>
            import('src/app/modules/admin/admin.module').then(m => m.AdminModule),
            data: {
                title:"Admin Console"
            }
    },
    {
        path: 'help',
        loadChildren: () =>
            import('src/app/modules/help/help.module').then(m => m.HelpModule),
            data: {
                title:"Help"
            }
    },
    {
        path: 'forbidden', component: ForbiddenComponent
    },
];

const config: ExtraOptions = {
    useHash: true,
    enableTracing: true,
    scrollPositionRestoration: 'enabled',
    initialNavigation: 'enabled',
    onSameUrlNavigation: 'reload'
    // ,preloadingStrategy: PreloadAllModules
    ,
    relativeLinkResolution: 'legacy'
};

@NgModule({
  imports: [RouterModule.forRoot(routes, config)],
  exports: [RouterModule],
  //providers: [{provide: LocationStrategy, useClass: HashLocationStrategy}]
  providers: [      
    {
      provide: RouteReuseStrategy,
      useClass: CustomRouteReuseStrategy
    }]

})
export class AppRoutingModule {

    

    //ngOnInit() {
    //    console.log(this.router.url);
    //    console.log(window.location.href);
    //}
}
