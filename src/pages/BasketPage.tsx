import { useEffect } from 'react';
import { useBasketStore } from '../store/basketStore';
import { BasketTableSkeleton } from '../components/Skeleton';
import { useToast } from '../components/Toast';
import type { BasketItem } from '../api/basket';

function getImageSrc(item: BasketItem): string {
  if (item.imageUrl) return item.imageUrl;
  if (item.imageFile) {
    if (item.imageFile.startsWith('http://') || item.imageFile.startsWith('https://')) return item.imageFile;
    return `/images/${item.imageFile}`;
  }
  return '/images/product-1.png';
}

export default function BasketPage() {
  const {
    items,
    totalItems,
    totalPrice,
    isLoading,
    fetchBasket,
    updateQuantity,
    removeItem,
    clearBasket,
  } = useBasketStore();
  const { showSuccess, showError } = useToast();

  useEffect(() => {
    fetchBasket();
  }, [fetchBasket]);

  const handleQuantityChange = async (item: BasketItem, newQuantity: number) => {
    if (newQuantity < 0) return;
    try {
      await updateQuantity(item.productId, newQuantity);
    } catch {
      showError('Error al actualizar cantidad');
    }
  };

  const handleRemove = async (item: BasketItem) => {
    if (!confirm(`¿Eliminar "${item.productName}" del carrito?`)) return;
    try {
      await removeItem(item.productId);
      showSuccess('Producto eliminado del carrito');
    } catch {
      showError('Error al eliminar del carrito');
    }
  };

  const handleClear = async () => {
    if (!confirm('¿Vaciar todo el carrito?')) return;
    try {
      await clearBasket();
      showSuccess('Carrito vaciado');
    } catch {
      showError('Error al vaciar carrito');
    }
  };

  if (isLoading && items.length === 0) {
    return (
      <div className="container-custom py-8">
        <h1 className="text-2xl font-bold mb-8">Carrito de Compras</h1>
        <BasketTableSkeleton />
      </div>
    );
  }

  return (
    <div className="container-custom py-8">
      <div className="flex items-center justify-between mb-8">
        <h1 className="text-2xl font-bold">Carrito de Compras</h1>
        {items.length > 0 && (
          <button
            onClick={handleClear}
            className="text-danger hover:text-danger-hover text-sm font-medium transition-colors"
          >
            Vaciar carrito
          </button>
        )}
      </div>

      {items.length === 0 ? (
        <div className="text-center py-12 text-gray-500 dark:text-gray-400">
          <svg className="w-16 h-16 mx-auto mb-4 opacity-50" fill="none" stroke="currentColor" viewBox="0 0 24 24">
            <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={1.5} d="M3 3h2l.4 2M7 13h10l4-8H5.4M7 13L5.4 5M7 13l-2.293 2.293c-.63.63-.184 1.707.707 1.707H17m0 0a2 2 0 100 4 2 2 0 000-4zm-8 2a2 2 0 100 4 2 2 0 000-4z" />
          </svg>
          <p className="text-lg">Tu carrito está vacío</p>
        </div>
      ) : (
        <div className="grid grid-cols-1 lg:grid-cols-3 gap-8">
          <div className="lg:col-span-2 space-y-4">
            {items.map((item) => (
              <div
                key={item.productId}
                className="bg-white dark:bg-gray-800 rounded-lg shadow-md p-4 flex gap-4"
              >
                <div className="w-20 h-20 sm:w-24 sm:h-24 bg-gray-100 dark:bg-gray-700 rounded-lg overflow-hidden flex-shrink-0">
                  <img
                    src={getImageSrc(item)}
                    alt={item.productName}
                    className="w-full h-full object-cover"
                    loading="lazy"
                    onError={(e) => {
                      (e.currentTarget as HTMLImageElement).src = '/images/product-1.png';
                    }}
                  />
                </div>
                <div className="flex-1 min-w-0">
                  <h3 className="font-semibold truncate">{item.productName}</h3>
                  <p className="text-sm text-gray-500 dark:text-gray-400">{item.color}</p>
                  <p className="text-primary font-bold mt-1">
                    ${(item.price ?? item.unitPrice ?? 0).toFixed(2)}
                  </p>
                </div>
                <div className="flex flex-col items-end gap-2">
                  <div className="flex items-center gap-2">
                    <button
                      onClick={() => handleQuantityChange(item, item.quantity - 1)}
                      className="w-8 h-8 border border-gray-300 dark:border-gray-600 rounded-lg hover:bg-gray-100 dark:hover:bg-gray-700 transition-colors flex items-center justify-center"
                    >
                      -
                    </button>
                    <span className="w-8 text-center font-medium">{item.quantity}</span>
                    <button
                      onClick={() => handleQuantityChange(item, item.quantity + 1)}
                      className="w-8 h-8 border border-gray-300 dark:border-gray-600 rounded-lg hover:bg-gray-100 dark:hover:bg-gray-700 transition-colors flex items-center justify-center"
                    >
                      +
                    </button>
                  </div>
                  <button
                    onClick={() => handleRemove(item)}
                    className="text-danger hover:text-danger-hover text-sm transition-colors"
                  >
                    Eliminar
                  </button>
                </div>
              </div>
            ))}
          </div>

          <div className="lg:sticky lg:top-8 h-fit">
            <div className="bg-white dark:bg-gray-800 rounded-lg shadow-md p-6">
              <h2 className="text-lg font-semibold mb-4">Resumen</h2>
              <div className="space-y-2 text-sm">
                <div className="flex justify-between">
                  <span className="text-gray-500 dark:text-gray-400">Productos</span>
                  <span>{totalItems}</span>
                </div>
                <div className="flex justify-between">
                  <span className="text-gray-500 dark:text-gray-400">Subtotal</span>
                  <span>${totalPrice.toFixed(2)}</span>
                </div>
                <div className="flex justify-between">
                  <span className="text-gray-500 dark:text-gray-400">Envío</span>
                  <span className="text-success">Gratis</span>
                </div>
              </div>
              <hr className="my-4 border-gray-200 dark:border-gray-700" />
              <div className="flex justify-between text-lg font-bold">
                <span>Total</span>
                <span>${totalPrice.toFixed(2)}</span>
              </div>
              <button className="w-full bg-primary hover:bg-primary-hover text-white py-3 rounded-lg mt-6 transition-colors font-medium">
                Proceder al pago
              </button>
            </div>
          </div>
        </div>
      )}
    </div>
  );
}
