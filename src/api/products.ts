import { catalogClient } from './client';

export interface Product {
  id: string;
  name: string;
  descripcion: string;
  category: string | string[];
  imageFile?: string;
  imageFiles?: string;
  imageUrl?: string;
  price: number;
}

export interface CreateProductRequest {
  name: string;
  description: string;
  category: string[];
  imagesFiles: string;
  imageUrl?: string;
  price: number;
}

export interface PaginatedResponse<T> {
  items: T[];
  totalCount: number;
  pageIndex: number;
  pageSize: number;
  totalPages: number;
  hasPreviousPage: boolean;
  hasNextPage: boolean;
}

export const getProducts = async (
  name?: string,
  pageIndex = 1,
  pageSize = 10
): Promise<PaginatedResponse<Product>> => {
  const params: Record<string, string | number> = { pageIndex, pageSize };
  if (name) params.name = name;

  const { data } = await catalogClient.get('/products', { params });

  const raw = data as Record<string, unknown>;
  const productsArray = (
    Array.isArray(raw.data) ? raw.data : Array.isArray(raw.items) ? raw.items : []
  ) as Product[];
  const totalCount = (raw.count ?? raw.totalCount ?? 0) as number;
  const pageIdx = (raw.pageIndex ?? 1) as number;
  const pageSz = (raw.pageSize ?? 10) as number;
  const totalPages = Math.max(1, Math.ceil(totalCount / pageSz));

  return {
    items: productsArray,
    totalCount,
    pageIndex: pageIdx,
    pageSize: pageSz,
    totalPages,
    hasPreviousPage: pageIdx > 1,
    hasNextPage: pageIdx < totalPages,
  };
};

export const createProduct = async (
  product: CreateProductRequest
): Promise<{ id: string }> => {
  const { data } = await catalogClient.post('/products', product);
  return data;
};

export const updateProduct = async (
  currentName: string,
  product: CreateProductRequest
): Promise<{ isSuccess: boolean }> => {
  const { data } = await catalogClient.put(`/products/${encodeURIComponent(currentName)}`, product);
  return data;
};

export const deleteProduct = async (
  name: string
): Promise<{ isSuccess: boolean }> => {
  const { data } = await catalogClient.delete(`/products/${encodeURIComponent(name)}`);
  return data;
};
