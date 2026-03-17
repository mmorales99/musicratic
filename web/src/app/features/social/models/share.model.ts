export type ShareType = 'hub' | 'track' | 'list' | 'profile';

export interface ShareLink {
  url: string;
  title: string;
  description: string;
  imageUrl: string | null;
}
