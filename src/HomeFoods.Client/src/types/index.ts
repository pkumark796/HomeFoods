export interface Product {
  id: string;
  name: string;
  description: string;
  sku: string;
  price: number;
  discountedPrice?: number;
  unit: string;
  weight: number;
  stockQuantity: number;
  isActive: boolean;
  isFeatured: boolean;
  imageUrl?: string;
  origin?: string;
  discountPercentage?: number;
  category?: Category;
  brand?: Brand;
}

export interface Category {
  id: string;
  name: string;
  description?: string;
  imageUrl?: string;
  displayOrder: number;
}

export interface Brand {
  id: string;
  name: string;
  description?: string;
  logoUrl?: string;
}

export interface CartItem {
  product: Product;
  quantity: number;
}

export interface Order {
  id: string;
  orderNumber: string;
  customerId: string;
  status: OrderStatus;
  subTotal: number;
  deliveryFee: number;
  tax: number;
  total: number;
  orderDate: string;
  items: OrderItem[];
}

export interface OrderItem {
  id: string;
  productId: string;
  product: Product;
  quantity: number;
  unitPrice: number;
  totalPrice: number;
}

export enum OrderStatus {
  Pending = 1,
  Processing = 2,
  Shipped = 3,
  Delivered = 4,
  Cancelled = 5,
}
