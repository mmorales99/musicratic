import { Injectable, inject } from "@angular/core";
import { Observable } from "rxjs";
import { BffApiService } from "@app/shared/services/bff-api.service";
import {
  UserProfile,
  UpdateProfileRequest,
} from "../models/profile.model";

@Injectable({ providedIn: "root" })
export class ProfileService {
  private readonly api = inject(BffApiService);

  getProfile(userId: string): Observable<UserProfile> {
    return this.api.get<UserProfile>(
      `/social/users/${encodeURIComponent(userId)}/profile`,
    );
  }

  updateProfile(profile: UpdateProfileRequest): Observable<UserProfile> {
    return this.api.put<UserProfile>("/social/users/me/profile", profile);
  }

  uploadAvatar(file: File): Observable<string> {
    const formData = new FormData();
    formData.append("avatar", file);
    return this.api.post<string>("/social/users/me/avatar", formData);
  }
}
