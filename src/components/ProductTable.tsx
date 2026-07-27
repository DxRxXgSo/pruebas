import type { Product } from '../api/products';

function getImageSrc(item: Product): string {
  if (item.imageUrl) return item.imageUrl;
  if (item.imageFile || item.imageFiles) {
    const f = item.imageFile || item.imageFiles || 'product-1.png';
    if (f.startsWith('http://') || f.startsWith('https://')) return f;
    return `/images/${f}`;
  }
  return '/images/product-1.png';
}

interface ProductTableProps {
  products: Product[];
  onEdit: (product: Product) => void;
  onDelete: (product: Product) => void;
  onAddToCart: (product: Product) => void;
}

export default function ProductTable({ products, onEdit, onDelete, onAddToCart }: ProductTableProps) {
  if (products.length === 0) {
    return (
      <div className="text-center py-12 text-gray-500 dark:text-gray-400">
        <svg className="w-16 h-16 mx-auto mb-4 opacity-50" fill="none" stroke="currentColor" viewBox="0 0 24 24">
          <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={1.5} d="M20 7l-8-4-8 4m16 0l-8 4m8-4v10l-8 4m0-10L4 7m8 4v10M4 7v10l8 4" />
        </svg>
        <p className="text-lg">No hay productos disponibles</p>
      </div>
    );
  }

  return (
    <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 xl:grid-cols-4 gap-6">
      {products.map((product) => (
        <div
          key={product.id}
          className="bg-white dark:bg-gray-800 rounded-lg shadow-md overflow-hidden hover:shadow-lg transition-shadow"
        >
          <div className="relative aspect-[4/3] bg-gray-100 dark:bg-gray-700 overflow-hidden">
            <img
              src={getImageSrc(product)}
              alt={product.name}
              className="w-full h-full object-cover"
              loading="lazy"
              onError={(e) => {
                (e.currentTarget as HTMLImageElement).src = '/images/product-1.png';
              }}
            />
          </div>
          <div className="p-4">
            <h3 className="font-semibold text-lg mb-1 truncate">{product.name}</h3>
            <p className="text-sm text-gray-500 dark:text-gray-400 mb-2 line-clamp-2">
              {product.descripcion}
            </p>
            <p className="text-xl font-bold text-primary mb-3">
              ${product.price.toFixed(2)}
            </p>
            <div className="flex gap-2">
              <button
                onClick={() => onAddToCart(product)}
                className="flex-1 bg-primary hover:bg-primary-hover text-white py-2 rounded-lg text-sm transition-colors"
              >
                Agregar
              </button>
              <button
                onClick={() => onEdit(product)}
                className="px-3 py-2 border border-gray-300 dark:border-gray-600 rounded-lg text-sm hover:bg-gray-100 dark:hover:bg-gray-700 transition-colors"
              >
                Editar
              </button>
              <button
                onClick={() => onDelete(product)}
                className="px-3 py-2 border border-danger text-danger rounded-lg text-sm hover:bg-danger hover:text-white transition-colors"
              >
                <svg className="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                  <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M19 7l-.867 12.142A2 2 0 0116.138 21H7.862a2 2 0 01-1.995-1.858L5 7m5 4v6m4-6v6m1-10V4a1 1 0 00-1-1h-4a1 1 0 00-1 1v3M4 7h16" />
                </svg>
              </button>
            </div>
          </div>
        </div>
      ))}
    </div>
  );
}
