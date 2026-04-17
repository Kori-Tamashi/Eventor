import type {
  Web_Dtos_Gender,
  Web_Dtos_UpdateUserRequest,
  Web_Dtos_UserRole,
} from '../generated';

export type CurrentUser = {
  id: string;
  name: string;
  phone: string;
  gender: Web_Dtos_Gender;
  role: Web_Dtos_UserRole;
  passwordHash?: string | null;
};

export type UpdateCurrentUserPayload = Web_Dtos_UpdateUserRequest;
