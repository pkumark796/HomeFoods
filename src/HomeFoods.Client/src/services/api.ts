import axios from 'axios';
import { Product, Category, Brand } from '../types';

// Resolve API base URL:
// 1. Use VITE_API_URL if provided
// 2. If running in browser on localhost, default to the API HTTPS dev port used by this solution
// 3. Otherwise use the same origin + /api
const envApiUrl = import.meta.env.VITE_API_URL;
let API_BASE_URL: string;
if (envApiUrl) {
  API_BASE_URL = envApiUrl;
} else if (typeof window !== 'undefined') {
  // Prefer the solution's HTTPS dev endpoint for localhost to match Swagger profile
  if (window.location.hostname === 'localhost') {
    API_BASE_URL = 'https://localhost:7033/api';
  } else {
    API_BASE_URL = `${window.location.origin}/api`;
  }
} else {
  API_BASE_URL = 'https://localhost:7033/api';
}

const api = axios.create({
  baseURL: API_BASE_URL,
  headers: {
    'Content-Type': 'application/json',
  },
});

export interface ProductFilterParams {
  searchTerm?: string;
  categoryId?: string;
  brandIds?: string[];
  minPrice?: number;
  maxPrice?: number;
  minDiscount?: number;
  inStock?: boolean;
  isFeatured?: boolean;
  sortBy?: 'name' | 'price' | 'discount' | 'newest' | 'popularity';
  sortDescending?: boolean;
  pageNumber?: number;
  pageSize?: number;
}

export interface PaginatedResponse<T> {
  items: T[];
  totalCount: number;
  pageNumber: number;
  pageSize: number;
  totalPages: number;
  hasPreviousPage: boolean;
  hasNextPage: boolean;
}

export const productService = {
  getAll: async (filters?: ProductFilterParams): Promise<PaginatedResponse<Product>> => {
    const params = new URLSearchParams();

    if (filters?.searchTerm) params.append('searchTerm', filters.searchTerm);
    if (filters?.categoryId) params.append('categoryId', filters.categoryId);
    if (filters?.brandIds) filters.brandIds.forEach(id => params.append('brandIds', id));
    if (filters?.minPrice !== undefined) params.append('minPrice', filters.minPrice.toString());
    if (filters?.maxPrice !== undefined) params.append('maxPrice', filters.maxPrice.toString());
    if (filters?.minDiscount !== undefined) params.append('minDiscount', filters.minDiscount.toString());
    if (filters?.inStock !== undefined) params.append('inStock', filters.inStock.toString());
    if (filters?.isFeatured !== undefined) params.append('isFeatured', filters.isFeatured.toString());
    if (filters?.sortBy) params.append('sortBy', filters.sortBy);
    if (filters?.sortDescending !== undefined) params.append('sortDescending', filters.sortDescending.toString());
    if (filters?.pageNumber) params.append('pageNumber', filters.pageNumber.toString());
    if (filters?.pageSize) params.append('pageSize', filters.pageSize.toString());

    const response = await api.get(`/products?${params.toString()}`);
    return response.data;
  },

  getById: async (id: string): Promise<Product> => {
    const response = await api.get(`/products/${id}`);
    return response.data;
  },

  getFeatured: async (): Promise<Product[]> => {
    const response = await api.get('/products/featured');
    return response.data;
  },

  getByCategory: async (categoryId: string): Promise<Product[]> => {
    const response = await api.get(`/products/category/${categoryId}`);
    return response.data;
  },

  getRelated: async (productId: string, limit: number = 4): Promise<Product[]> => {
    const response = await api.get(`/products/${productId}/related?limit=${limit}`);
    return response.data;
  },

  search: async (searchTerm: string): Promise<Product[]> => {
    const response = await api.get(`/products/search?q=${encodeURIComponent(searchTerm)}`);
    return response.data;
  },

  getSuggestions: async (searchTerm: string, limit: number = 10): Promise<string[]> => {
    const response = await api.get(`/products/suggestions?q=${encodeURIComponent(searchTerm)}&limit=${limit}`);
    return response.data;
  },
};

export const categoryService = {
  getAll: async (): Promise<Category[]> => {
    const response = await api.get('/categories');
    return response.data;
  },

  getById: async (id: string): Promise<Category> => {
    const response = await api.get(`/categories/${id}`);
    return response.data;
  },
};

export const brandService = {
  getAll: async (): Promise<Brand[]> => {
    const response = await api.get('/brands');
    return response.data;
  },
};

export interface CheckoutRequest {
  customerId: string;
  deliveryAddressId: string;
  paymentMethod: number;
  specialInstructions: string | null;
  items: Array<{
    productId: string;
    quantity: number;
  }>;
}

export interface CheckoutResponse {
  orderId: string;
  orderNumber: string;
  total: number;
  status: string;
  orderDate: string;
  payment?: {
    paymentId: string;
    status: string;
    transactionId?: string;
    phonePeUrl?: string;
  };
}

export const checkoutService = {
  checkout: async (data: CheckoutRequest): Promise<CheckoutResponse> => {
    const response = await api.post('/checkout', data);
    return response.data;
  },
};

export default api;

export const chatService = {
  startChat: async (): Promise<{ sessionId: string }> => {
    const response = await api.post('/chat/start');
    return response.data;
  },

  sendMessage: async (sessionId: string, message: string): Promise<{ reply: string }> => {
    const response = await api.post('/chat/message', { sessionId, message });
    return response.data;
  },
};
