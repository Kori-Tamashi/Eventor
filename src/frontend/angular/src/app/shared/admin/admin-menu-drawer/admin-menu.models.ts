import { ItemApiModel } from '../../../core/api/services/items-api.service';

export type AdminMenuEditorMode = 'create' | 'edit';

export type AdminMenuEditorItem = {
  itemId: string;
  title: string;
  amount: string;
};

export type AdminMenuDraftValue = {
  title: string;
  description: string;
  items: AdminMenuEditorItem[];
};

export type AdminMenuSavePayload = {
  title: string;
  description?: string | null;
  items: Array<{
    itemId: string;
    title: string;
    amount: number;
  }>;
};

export type AdminMenuDrawerData = {
  draft: AdminMenuDraftValue;
  availableItems: ItemApiModel[];
};
