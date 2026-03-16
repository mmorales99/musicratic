import { Component, ChangeDetectionStrategy } from "@angular/core";

@Component({
  selector: "app-profile",
  standalone: true,
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    <section class="profile-page">
      <h1>Profile</h1>
      <p>View and edit your profile settings.</p>
    </section>
  `,
})
export class ProfileComponent {}
