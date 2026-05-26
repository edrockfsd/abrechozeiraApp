export interface Permission {
  id: string;
  name: string;
  description: string;
  resource: string;
  action: string;
  isActive: boolean;
}

export enum PermissionAction {
  CREATE = 'CREATE',
  READ = 'READ',
  UPDATE = 'UPDATE',
  DELETE = 'DELETE',
  MANAGE = 'MANAGE'
}

export enum PermissionResource {
  PRODUCTS = 'PRODUCTS',
  ORDERS = 'ORDERS',
  CUSTOMERS = 'CUSTOMERS',
  INVENTORY = 'INVENTORY',
  LIVE_SESSIONS = 'LIVE_SESSIONS',
  AUCTIONS = 'AUCTIONS',
  USERS = 'USERS',
  ROLES = 'ROLES',
  REPORTS = 'REPORTS'
}