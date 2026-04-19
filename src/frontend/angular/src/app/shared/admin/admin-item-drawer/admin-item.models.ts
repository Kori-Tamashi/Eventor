export type AdminItemEditorMode = 'create' | 'edit';

export type AdminItemDraftValue = {
  title: string;
  cost: string;
  calculatedCost?: string;
};

export type AdminItemSavePayload = {
  title: string;
  cost: number;
};
