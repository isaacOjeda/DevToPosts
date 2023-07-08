import { Directive, Input, TemplateRef, ViewContainerRef } from '@angular/core';
import { AuthenticationService } from './authentication.service';

@Directive({
  selector: '[appAuthorize]'
})
export class AuthorizeDirective {


  constructor(
    private templateRef: TemplateRef<any>,
    private viewContainer: ViewContainerRef,
    private authService: AuthenticationService
  ) { }


  @Input() set appAuthorize(roleName: string) {

    if (!this.authService.isInRole(roleName)) {
      this.viewContainer.clear();
    } else {
      this.viewContainer.createEmbeddedView(this.templateRef);
    }
  }

}
