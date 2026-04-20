export type AdminLocationEditorMode = 'create' | 'edit';

export type AdminLocationDraftValue = {
  title: string;
  description: string;
  cost: string;
  capacity: string;
};

export type AdminLocationSavePayload = {
  title: string;
  description?: string | null;
  cost: number;
  capacity: number;
};
