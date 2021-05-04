import { BrowserModule } from '@angular/platform-browser';
import { NgModule, APP_INITIALIZER } from '@angular/core';
import { AppRoutingModule } from './app-routing.module';
import { AppComponent } from './app.component';
import { BrowserAnimationsModule } from '@angular/platform-browser/animations';
import { ModFrameworkModule,  ModFrameworkConfig } from 'mod-framework';
import {MaterialModule} from './material-module';
import { BreadcrumbComponent } from './breadcrumb/breadcrumb.component';
import { DataSources } from './modules/shared/interfaces/data-source';
import { AppConfigService } from './services/AppConfigService';
import { environment } from 'src/environments/environment';
import { EventEmitterService } from './services/event-emitter.service';
import { HTTP_INTERCEPTORS } from '@angular/common/http';
import { NoCacheHeadersInterceptor } from './Directives/NoCacheHeadersInterceptor';

export function appInit(appConfigService: AppConfigService ) {
    return () => appConfigService.load();
}

const modConfig: ModFrameworkConfig = {
    loginSiteUrl: environment.apiUrl,
    loadingDelay: 1000,
    urlsToSkip: [],    
    showHelp: true,
    helpOptions: [
        'Introduction to CATS', 'How to Navigate the Correspondence Dashboard','How to Navigate the Originator Dashboard','How to Navigate the Reviewer Dashboard',
        'How to Launch a New Collaboration','How to Manage a Review Round','How to Move Packages to the Next Round','How to Review a Document','How to Assign Surrogates',
        'Correspondence and Clearance Business Process Overview','Roles and Responsibilities','Business Process Rules','Correspondence Letter Review Process','720 Letter Review Process',
        'Memoranda, Guidance and Reports Review Process','Federal Register Notices Review Process','ADA Reports Review Process',
        'OMB Executive Secretary Resources','Email CATS System Operator for assistance'
    ],    
    showSearch: false,
    userOptions: ['Account Settings','Profile'],
    profileUrl: "https://portfolio.omb.gov/portfolio"
}

@NgModule({
    declarations: [
        AppComponent,
        BreadcrumbComponent
    ],
    exports: [MaterialModule],
    imports: [
        BrowserModule,
        AppRoutingModule,
        BrowserAnimationsModule,
        ModFrameworkModule.forRoot(modConfig),
        MaterialModule
    ],
    //providers: [{ provide: RouteReuseStrategy, useClass: AppRoutingCache }, DataSources],
    providers: [DataSources, AppConfigService, EventEmitterService,
        {
            provide: APP_INITIALIZER,
            useFactory: appInit,
            multi: true,
            deps: [AppConfigService]
        },
        {
            provide: HTTP_INTERCEPTORS,
            useClass: NoCacheHeadersInterceptor,
            multi: true
        }
    ],
    bootstrap: [AppComponent]
})
export class AppModule { }
