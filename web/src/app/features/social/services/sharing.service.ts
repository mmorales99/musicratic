import { Injectable, inject } from "@angular/core";
import { Observable, firstValueFrom } from "rxjs";
import { BffApiService } from "@app/shared/services/bff-api.service";
import { ShareLink, ShareType } from "../models/share.model";

@Injectable({ providedIn: "root" })
export class SharingService {
  private readonly api = inject(BffApiService);

  getShareLink(type: ShareType, entityId: string): Observable<ShareLink> {
    const encodedType = encodeURIComponent(type);
    const encodedId = encodeURIComponent(entityId);
    return this.api.get<ShareLink>(`/social/share/${encodedType}/${encodedId}`);
  }

  async share(shareLink: ShareLink): Promise<void> {
    if (this.canUseNativeShare()) {
      await navigator.share({
        title: shareLink.title,
        text: shareLink.description,
        url: shareLink.url,
      });
    } else {
      await navigator.clipboard.writeText(shareLink.url);
    }
  }

  canUseNativeShare(): boolean {
    return typeof navigator !== "undefined" && "share" in navigator;
  }
}
