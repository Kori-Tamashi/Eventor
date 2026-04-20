import { Web_Dtos_Gender, Web_Dtos_UserRole } from '../../../core/api/generated';

export type AdminUserEditorMode = 'create' | 'edit';

export type AdminUserDraftValue = {
  name: string;
  phone: string;
  gender: Web_Dtos_Gender;
  role: Web_Dtos_UserRole;
  password: string;
};

export type AdminUserSavePayload = {
  name: string;
  phone: string;
  gender: Web_Dtos_Gender;
  role: Web_Dtos_UserRole;
  password?: string;
};
