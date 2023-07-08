import { BrowserModule } from '@angular/platform-browser';
import { NgModule } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { HttpClientModule } from '@angular/common/http';
import { RouterModule } from '@angular/router';

import { AppComponent } from './app.component';
import { NavMenuComponent } from './shared/layout/nav-menu/nav-menu.component';
import { HomeComponent } from './home/home.component';
import { LoginComponent } from './login/login.component';
import { LayoutComponent } from './shared/layout/layout.component';
import { ProductsComponent } from './products/products.component';
import { AuthGuard } from './shared/auth.guard';
import { AuthorizeDirective } from './shared/authorize.directive';

@NgModule({
  declarations: [
    AppComponent,
    NavMenuComponent,
    HomeComponent,
    ProductsComponent,
    LoginComponent,
    LayoutComponent,
    AuthorizeDirective
  ],
  imports: [
    BrowserModule.withServerTransition({ appId: 'ng-cli-universal' }),
    HttpClientModule,
    FormsModule,
    RouterModule.forRoot([
      {
        path: '',
        component: LayoutComponent,
        children: [
          { path: '', component: HomeComponent },
          { path: 'products', component: ProductsComponent, canActivate: [AuthGuard] }
        ]
      },
      { path: 'login', component: LoginComponent },
      { path: '**', redirectTo: '/', pathMatch: 'full' },
    ])
  ],
  providers: [],
  bootstrap: [AppComponent]
})
export class AppModule { }
