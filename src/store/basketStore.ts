import { create } from 'zustand';
import { persist } from 'zustand/middleware';
import { BasketItem, getBasket, storeBasket, deleteBasket } from '../api/basket';
import type { Product } from '../api/products';

const USER_NAME = 'comprador1';

const calculateTotals = (items: BasketItem[]) => {
  const totalItems = items.reduce((sum, item) => sum + item.quantity, 0);
  const totalPrice = items.reduce(
    (sum, item) => sum + ((item.price ?? item.unitPrice ?? 0) * item.quantity),
    0
  );
  return { totalItems, totalPrice };
};

interface BasketState {
  items: BasketItem[];
  totalItems: number;
  totalPrice: number;
  isLoading: boolean;
  error: string | null;

  fetchBasket: () => Promise<void>;
  addItem: (product: Product, quantity?: number, color?: string) => Promise<void>;
  updateQuantity: (productId: string, quantity: number) => Promise<void>;
  removeItem: (productId: string) => Promise<void>;
  clearBasket: () => Promise<void>;
}

export const useBasketStore = create<BasketState>()(
  persist(
    (set, get) => ({
      items: [],
      totalItems: 0,
      totalPrice: 0,
      isLoading: false,
      error: null,

      fetchBasket: async () => {
        set({ isLoading: true, error: null });
        try {
          const basket = await getBasket(USER_NAME);
          if (basket && basket.items) {
            const { totalItems, totalPrice } = calculateTotals(basket.items);
            set({ items: basket.items, totalItems, totalPrice, isLoading: false });
          }
        } catch {
          set({ isLoading: false });
        }
      },

      addItem: async (product: Product, quantity = 1, color = '') => {
        const { items: previousItems, totalItems: previousTotalItems, totalPrice: previousTotalPrice } = get();
        set({ isLoading: true, error: null });

        try {
          const imageFile = product.imageFile || product.imageFiles || product.imageUrl || 'product-1.png';
          const imageUrl = product.imageUrl || '';
          const category = Array.isArray(product.category) ? product.category[0] || '' : product.category || '';

          const existingIndex = previousItems.findIndex(
            (item) => item.productId === product.id && item.color === color
          );

          let newItems: BasketItem[];
          if (existingIndex >= 0) {
            newItems = [...previousItems];
            newItems[existingIndex] = {
              ...newItems[existingIndex],
              quantity: newItems[existingIndex].quantity + quantity,
            };
          } else {
            const newItem: BasketItem = {
              productId: product.id,
              productName: product.name,
              price: product.price,
              quantity,
              color: color || category,
              imageFile,
              imageUrl,
            };
            newItems = [...previousItems, newItem];
          }

          const totals = calculateTotals(newItems);
          set({ items: newItems, ...totals, isLoading: false });

          await storeBasket(USER_NAME, newItems);
        } catch (error) {
          set({
            items: previousItems,
            totalItems: previousTotalItems,
            totalPrice: previousTotalPrice,
            error: 'Error al agregar al carrito',
            isLoading: false,
          });
          throw error;
        }
      },

      updateQuantity: async (productId: string, quantity: number) => {
        const { items: previousItems } = get();
        set({ isLoading: true, error: null });

        try {
          const newItems = quantity <= 0
            ? previousItems.filter((item) => item.productId !== productId)
            : previousItems.map((item) =>
                item.productId === productId ? { ...item, quantity } : item
              );

          const totals = calculateTotals(newItems);
          set({ items: newItems, ...totals, isLoading: false });

          await storeBasket(USER_NAME, newItems);
        } catch (error) {
          set({ items: previousItems, error: 'Error al actualizar cantidad', isLoading: false });
          throw error;
        }
      },

      removeItem: async (productId: string) => {
        const { items: previousItems } = get();
        set({ isLoading: true, error: null });

        try {
          const newItems = previousItems.filter((item) => item.productId !== productId);
          const totals = calculateTotals(newItems);
          set({ items: newItems, ...totals, isLoading: false });

          if (newItems.length === 0) {
            await deleteBasket(USER_NAME);
          } else {
            await storeBasket(USER_NAME, newItems);
          }
        } catch (error) {
          set({ items: previousItems, error: 'Error al eliminar del carrito', isLoading: false });
          throw error;
        }
      },

      clearBasket: async () => {
        const { items: previousItems } = get();
        set({ isLoading: true, error: null });

        try {
          set({ items: [], totalItems: 0, totalPrice: 0, isLoading: false });
          await deleteBasket(USER_NAME);
        } catch (error) {
          set({ items: previousItems, error: 'Error al vaciar carrito', isLoading: false });
          throw error;
        }
      },
    }),
    {
      name: 'basket-storage',
      partialize: (state) => ({
        items: state.items,
        totalItems: state.totalItems,
        totalPrice: state.totalPrice,
      }),
    }
  )
);
