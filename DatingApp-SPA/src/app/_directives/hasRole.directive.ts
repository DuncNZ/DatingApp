import { Directive, Input, ViewContainerRef, TemplateRef, OnInit } from '@angular/core';
import { AuthService } from '../_services/auth.service';

@Directive({
  selector: '[appHasRole]'  // *appHasRole
})
export class HasRoleDirective implements OnInit {
  @Input() appHasRole: string[];
  isVisible = false;

  constructor(
    private viewContainerRef: ViewContainerRef,
    private templateRef: TemplateRef<any>,
    private authService: AuthService
  ) { }

  ngOnInit() {
    const userRoles = this.authService.decodedToken.role as Array<string>;
    // If no roles, clear view container ref
    if (!userRoles) {
      this.viewContainerRef.clear();
    }

    // Does user have the right role
    if (this.authService.roleMatch(this.appHasRole)) {
      if (!this.isVisible) {
        this.isVisible = true;
        this.viewContainerRef.createEmbeddedView(this.templateRef);
      } else {
        this.isVisible = false;
        this.viewContainerRef.clear();
      }
    }
  }
}
