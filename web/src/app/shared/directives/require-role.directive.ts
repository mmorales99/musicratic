import {
  Directive,
  Input,
  TemplateRef,
  ViewContainerRef,
  inject,
  effect,
  OnDestroy,
} from "@angular/core";
import { RoleService } from "@app/shared/services/role.service";
import { UserRole } from "@app/shared/models/user-role.model";

@Directive({
  selector: "[appRequireRole]",
  standalone: true,
})
export class RequireRoleDirective implements OnDestroy {
  private readonly templateRef = inject(TemplateRef<unknown>);
  private readonly viewContainer = inject(ViewContainerRef);
  private readonly roleService = inject(RoleService);
  private hasView = false;
  private requiredRole: UserRole = UserRole.Anonymous;
  private readonly effectRef;

  @Input()
  set appRequireRole(role: UserRole | keyof typeof UserRole) {
    this.requiredRole = typeof role === "string" ? UserRole[role] : role;
    this.updateView();
  }

  constructor() {
    this.effectRef = effect(() => {
      // Re-evaluate whenever currentRole signal changes
      this.roleService.currentRole();
      this.updateView();
    });
  }

  ngOnDestroy(): void {
    this.effectRef.destroy();
  }

  private updateView(): void {
    const shouldShow = this.roleService.hasRole(this.requiredRole);

    if (shouldShow && !this.hasView) {
      this.viewContainer.createEmbeddedView(this.templateRef);
      this.hasView = true;
    } else if (!shouldShow && this.hasView) {
      this.viewContainer.clear();
      this.hasView = false;
    }
  }
}
