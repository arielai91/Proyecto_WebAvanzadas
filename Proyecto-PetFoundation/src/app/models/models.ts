// Auth Models
export interface LoginRequest {
  email: string;
  password: string;
}

export interface RegisterRequest {
  email: string;
  password: string;
  name: string;
  phone?: string;
  address?: string;
}

export interface AuthResponse {
  token: string;
  userId: number;
  email: string;
  name: string;
  roles: string[];
}

export interface UserProfile {
  userId: string;
  email: string;
  name: string;
  roles: string[];
}

// Pet Models
export interface Pet {
  id: number;
  name: string;
  species: string;
  breed?: string;
  age?: number;
  sex: string;
  size: string;
  color?: string;
  description?: string;
  status: string;
  coverImageUrl?: string;
  createdById: number;
  createdAt: Date;
  updatedAt?: Date;
}

export interface CreatePetRequest {
  name: string;
  species: string;
  breed?: string;
  age?: number;
  sex: string;
  size: string;
  color?: string;
  description?: string;
}

export interface UpdatePetRequest {
  name: string;
  species: string;
  breed?: string;
  age?: number;
  sex: string;
  size: string;
  color?: string;
  description?: string;
  status: string;
}

export interface PatchPetRequest {
  name?: string;
  species?: string;
  breed?: string;
  age?: number;
  sex?: string;
  size?: string;
  color?: string;
  description?: string;
  status?: string;
}

export interface PaginatedPets {
  totalCount: number;
  page: number;
  pageSize: number;
  items: Pet[];
}

export interface PetFilters {
  page?: number;
  pageSize?: number;
  status?: string;
  species?: string;
  size?: string;
  sex?: string;
  createdById?: number;
  minAge?: number;
  maxAge?: number;
  search?: string;
  createdFrom?: Date;
  createdTo?: Date;
}

// Adoption Request Models
export interface AdoptionRequest {
  id: number;
  petId: number;
  userId: number;
  status: string;
  message: string;
  decisionById?: number;
  decisionDate?: Date;
  createdAt: Date;
  updatedAt?: Date;
  pet?: Pet;
  user?: any;
  decisionBy?: any;
}

export interface CreateAdoptionRequestRequest {
  petId: number;
  message: string;
}

export interface PaginatedAdoptionRequests {
  totalCount: number;
  page: number;
  pageSize: number;
  items: AdoptionRequest[];
}

export interface AdoptionRequestFilters {
  page?: number;
  pageSize?: number;
  status?: string;
  petId?: number;
  userId?: number;
  decisionById?: number;
  createdFrom?: Date;
  createdTo?: Date;
}

// Notification Models
export interface Notification {
  id: number;
  userId: number;
  type: string;
  title: string;
  message: string;
  isRead: boolean;
  createdAt: Date;
  data?: any;
}

export interface PaginatedNotifications {
  totalCount: number;
  page: number;
  pageSize: number;
  items: Notification[];
}

export interface NotificationFilters {
  page?: number;
  pageSize?: number;
  type?: string;
  isRead?: boolean;
  createdFrom?: Date;
  createdTo?: Date;
}

// Constants
export const PET_STATUSES = {
  AVAILABLE: 'Available',
  PENDING: 'Pending',
  ADOPTED: 'Adopted'
};

export const PET_SIZES = {
  PEQUENO: 'Pequeño',
  MEDIANO: 'Mediano',
  GRANDE: 'Grande'
};

export const PET_SEXES = {
  MACHO: 'Macho',
  HEMBRA: 'Hembra'
};

export const ADOPTION_STATUSES = {
  PENDING: 'Pending',
  APPROVED: 'Approved',
  REJECTED: 'Rejected',
  CANCELLED: 'Cancelled'
};
