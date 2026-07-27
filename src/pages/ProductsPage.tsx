import { useState, useEffect, useCallback } from 'react';
import { getProducts, deleteProduct, createProduct, updateProduct } from '../api/products';
import type { Product, CreateProductRequest } from '../api/products';
import ProductTable from '../components/ProductTable';
import ProductForm from '../components/ProductForm';
import Modal from '../components/Modal';
import { ProductTableSkeleton } from '../components/Skeleton';
import { useToast } from '../components/Toast';
import { useBasketStore } from '../store/basketStore';

export default function ProductsPage() {
  const [products, setProducts] = useState<Product[]>([]);
  const [isLoading, setIsLoading] = useState(true);
  const [search, setSearch] = useState('');
  const [pageIndex, setPageIndex] = useState(1);
  const [pageSize] = useState(12);
  const [totalPages, setTotalPages] = useState(1);
  const [totalCount, setTotalCount] = useState(0);
  const [isModalOpen, setIsModalOpen] = useState(false);
  const [editingProduct, setEditingProduct] = useState<Product | null>(null);

  const { showSuccess, showError } = useToast();
  const addItem = useBasketStore((state) => state.addItem);

  const fetchProducts = useCallback(async () => {
    setIsLoading(true);
    try {
      const result = await getProducts(search || undefined, pageIndex, pageSize);
      setProducts(result.items);
      setTotalPages(result.totalPages);
      setTotalCount(result.totalCount);
    } catch {
      showError('Error al cargar productos');
    } finally {
      setIsLoading(false);
    }
  }, [search, pageIndex, pageSize, showError]);

  useEffect(() => {
    fetchProducts();
  }, [fetchProducts]);

  const handleSearch = (e: React.FormEvent) => {
    e.preventDefault();
    setPageIndex(1);
    fetchProducts();
  };

  const handleCreate = () => {
    setEditingProduct(null);
    setIsModalOpen(true);
  };

  const handleEdit = (product: Product) => {
    setEditingProduct(product);
    setIsModalOpen(true);
  };

  const handleDelete = async (product: Product) => {
    if (!confirm(`¿Eliminar "${product.name}"?`)) return;
    try {
      await deleteProduct(product.name);
      showSuccess('Producto eliminado');
      fetchProducts();
    } catch {
      showError('Error al eliminar producto');
    }
  };

  const handleAddToCart = async (product: Product) => {
    try {
      await addItem(product);
      showSuccess(`${product.name} agregado al carrito`);
    } catch {
      showError('Error al agregar al carrito');
    }
  };

  const handleSubmit = async (data: CreateProductRequest) => {
    try {
      if (editingProduct) {
        await updateProduct(editingProduct.name, data);
        showSuccess('Producto actualizado');
      } else {
        await createProduct(data);
        showSuccess('Producto creado');
      }
      setIsModalOpen(false);
      fetchProducts();
    } catch {
      showError(editingProduct ? 'Error al actualizar producto' : 'Error al crear producto');
    }
  };

  return (
    <div className="container-custom py-8">
      <div className="flex flex-col sm:flex-row sm:items-center sm:justify-between gap-4 mb-8">
        <h1 className="text-2xl font-bold">Catálogo de Productos</h1>
        <button
          onClick={handleCreate}
          className="bg-primary hover:bg-primary-hover text-white px-4 py-2 rounded-lg transition-colors"
        >
          + Nuevo Producto
        </button>
      </div>

      <form onSubmit={handleSearch} className="mb-6 flex gap-2">
        <input
          type="text"
          value={search}
          onChange={(e) => setSearch(e.target.value)}
          placeholder="Buscar productos..."
          className="flex-1 px-4 py-2 border border-gray-300 dark:border-gray-600 rounded-lg bg-white dark:bg-gray-800 focus:ring-2 focus:ring-primary focus:border-transparent outline-none"
        />
        <button
          type="submit"
          className="bg-primary hover:bg-primary-hover text-white px-4 py-2 rounded-lg transition-colors"
        >
          Buscar
        </button>
      </form>

      {isLoading ? (
        <ProductTableSkeleton />
      ) : (
        <>
          <ProductTable
            products={products}
            onEdit={handleEdit}
            onDelete={handleDelete}
            onAddToCart={handleAddToCart}
          />

          {totalPages > 1 && (
            <div className="flex items-center justify-center gap-2 mt-8">
              <button
                onClick={() => setPageIndex((p) => Math.max(1, p - 1))}
                disabled={pageIndex <= 1}
                className="px-3 py-1 border rounded-lg disabled:opacity-50 hover:bg-gray-100 dark:hover:bg-gray-700 transition-colors"
              >
                Anterior
              </button>
              <span className="text-sm text-gray-600 dark:text-gray-400">
                Página {pageIndex} de {totalPages} ({totalCount} productos)
              </span>
              <button
                onClick={() => setPageIndex((p) => Math.min(totalPages, p + 1))}
                disabled={pageIndex >= totalPages}
                className="px-3 py-1 border rounded-lg disabled:opacity-50 hover:bg-gray-100 dark:hover:bg-gray-700 transition-colors"
              >
                Siguiente
              </button>
            </div>
          )}
        </>
      )}

      <Modal
        isOpen={isModalOpen}
        onClose={() => setIsModalOpen(false)}
        title={editingProduct ? 'Editar Producto' : 'Nuevo Producto'}
      >
        <ProductForm
          initial={editingProduct}
          onSubmit={handleSubmit}
          onCancel={() => setIsModalOpen(false)}
        />
      </Modal>
    </div>
  );
}
