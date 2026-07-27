import { basketClient } from './client';

export interface BasketItem {
  quantity: number;
  color: string;
  price: number;
  unitPrice?: number;
  productId: string;
  productName: string;
  imageFile: string;
  imageUrl: string;
}

export interface Basket {
  userName: string;
  items: BasketItem[];
  totalPrice: number;
  tatalPrice?: number;
}

export interface StoreBasketPayload {
  cart: {
    userName: string;
    items: BasketItem[];
  };
}

export const getBasket = async (userName: string): Promise<Basket> => {
  const { data } = await basketClient.get(`/basket/${encodeURIComponent(userName)}`);
  return data;
};

export const storeBasket = async (
  userName: string,
  items: BasketItem[]
): Promise<{ userName: string }> => {
  const payload: StoreBasketPayload = {
    cart: { userName, items },
  };
  const { data } = await basketClient.post('/basket', payload);
  return data;
};

export const deleteBasket = async (
  userName: string
): Promise<{ isSuccess: boolean }> => {
  const { data } = await basketClient.delete(`/basket/${encodeURIComponent(userName)}`);
  return data;
};
