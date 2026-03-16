import { Component, ChangeDetectionStrategy } from "@angular/core";
import { RouterOutlet } from "@angular/router";
import { ShellComponent } from "./shell/shell.component";
import { ToastContainerComponent } from "./shared/components/toast/toast-container.component";

@Component({
  selector: "app-root",
  standalone: true,
  imports: [RouterOutlet, ShellComponent, ToastContainerComponent],
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    <app-shell>
      <router-outlet />
    </app-shell>
    <app-toast-container />
  `,
})
export class AppComponent {}
