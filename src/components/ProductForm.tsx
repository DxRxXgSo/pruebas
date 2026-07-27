import { useState, useEffect } from 'react';
import type { Product, CreateProductRequest } from '../api/products';

interface ProductFormProps {
  initial?: Product | null;
  onSubmit: (data: CreateProductRequest) => Promise<void>;
  onCancel: () => void;
}

export default function ProductForm({ initial, onSubmit, onCancel }: ProductFormProps) {
  const [name, setName] = useState('');
  const [description, setDescription] = useState('');
  const [category, setCategory] = useState('');
  const [imageFile, setImageFile] = useState('');
  const [imageUrl, setImageUrl] = useState('');
  const [price, setPrice] = useState('');
  const [submitting, setSubmitting] = useState(false);

  useEffect(() => {
    if (initial) {
      setName(initial.name || '');
      setDescription(initial.descripcion || '');
      setCategory(
        Array.isArray(initial.category) ? initial.category.join(', ') : initial.category || ''
      );
      setImageFile(initial.imageFile || initial.imageFiles || '');
      setImageUrl(initial.imageUrl || '');
      setPrice(String(initial.price || ''));
    }
  }, [initial]);

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!name.trim() || !price) return;

    setSubmitting(true);
    try {
      await onSubmit({
        name: name.trim(),
        description: description.trim(),
        category: category.split(',').map((c) => c.trim()).filter(Boolean),
        imagesFiles: imageFile.trim() || 'product-1.png',
        imageUrl: imageUrl.trim() || undefined,
        price: Number(price),
      });
    } finally {
      setSubmitting(false);
    }
  };

  return (
    <form onSubmit={handleSubmit} className="space-y-4">
      <div>
        <label className="block text-sm font-medium mb-1">Nombre</label>
        <input
          type="text"
          value={name}
          onChange={(e) => setName(e.target.value)}
          required
          className="w-full px-3 py-2 border border-gray-300 dark:border-gray-600 rounded-lg bg-white dark:bg-gray-700 focus:ring-2 focus:ring-primary focus:border-transparent outline-none"
          placeholder="Nombre del producto"
        />
      </div>

      <div>
        <label className="block text-sm font-medium mb-1">Descripción</label>
        <textarea
          value={description}
          onChange={(e) => setDescription(e.target.value)}
          rows={3}
          className="w-full px-3 py-2 border border-gray-300 dark:border-gray-600 rounded-lg bg-white dark:bg-gray-700 focus:ring-2 focus:ring-primary focus:border-transparent outline-none"
          placeholder="Descripción del producto"
        />
      </div>

      <div>
        <label className="block text-sm font-medium mb-1">Categorías (separadas por coma)</label>
        <input
          type="text"
          value={category}
          onChange={(e) => setCategory(e.target.value)}
          className="w-full px-3 py-2 border border-gray-300 dark:border-gray-600 rounded-lg bg-white dark:bg-gray-700 focus:ring-2 focus:ring-primary focus:border-transparent outline-none"
          placeholder="Electronica, Computo"
        />
      </div>

      <div>
        <label className="block text-sm font-medium mb-1">Archivo de imagen</label>
        <input
          type="text"
          value={imageFile}
          onChange={(e) => setImageFile(e.target.value)}
          className="w-full px-3 py-2 border border-gray-300 dark:border-gray-600 rounded-lg bg-white dark:bg-gray-700 focus:ring-2 focus:ring-primary focus:border-transparent outline-none"
          placeholder="product-1.png"
        />
      </div>

      <div>
        <label className="block text-sm font-medium mb-1">URL de imagen externa</label>
        <input
          type="url"
          value={imageUrl}
          onChange={(e) => setImageUrl(e.target.value)}
          className="w-full px-3 py-2 border border-gray-300 dark:border-gray-600 rounded-lg bg-white dark:bg-gray-700 focus:ring-2 focus:ring-primary focus:border-transparent outline-none"
          placeholder="https://picsum.photos/300/200"
        />
      </div>

      <div>
        <label className="block text-sm font-medium mb-1">Precio</label>
        <input
          type="number"
          step="0.01"
          min="0"
          value={price}
          onChange={(e) => setPrice(e.target.value)}
          required
          className="w-full px-3 py-2 border border-gray-300 dark:border-gray-600 rounded-lg bg-white dark:bg-gray-700 focus:ring-2 focus:ring-primary focus:border-transparent outline-none"
          placeholder="99.99"
        />
      </div>

      <div className="flex gap-3 pt-2">
        <button
          type="submit"
          disabled={submitting}
          className="flex-1 bg-primary hover:bg-primary-hover text-white py-2 rounded-lg transition-colors disabled:opacity-50"
        >
          {submitting ? 'Guardando...' : initial ? 'Actualizar' : 'Crear'}
        </button>
        <button
          type="button"
          onClick={onCancel}
          className="px-4 py-2 border border-gray-300 dark:border-gray-600 rounded-lg hover:bg-gray-100 dark:hover:bg-gray-700 transition-colors"
        >
          Cancelar
        </button>
      </div>
    </form>
  );
}
