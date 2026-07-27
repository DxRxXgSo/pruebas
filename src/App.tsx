import { Link, Outlet, useLocation } from 'react-router-dom';
import { useBasketStore } from './store/basketStore';

export default function App() {
  const location = useLocation();
  const totalItems = useBasketStore((state) => state.totalItems);

  const isActive = (path: string) =>
    location.pathname === path
      ? 'text-primary font-semibold'
      : 'text-gray-600 dark:text-gray-300 hover:text-primary dark:hover:text-primary';

  return (
    <div className="min-h-screen flex flex-col">
      <nav className="bg-white dark:bg-gray-800 shadow-sm border-b border-gray-200 dark:border-gray-700">
        <div className="container-custom flex items-center justify-between h-16">
          <Link to="/products" className="text-xl font-bold text-primary">
            E-Shop
          </Link>
          <div className="flex items-center gap-6">
            <Link to="/products" className={`${isActive('/products')} transition-colors`}>
              Productos
            </Link>
            <Link to="/basket" className={`${isActive('/basket')} transition-colors relative`}>
              Carrito
              {totalItems > 0 && (
                <span className="absolute -top-2 -right-4 bg-danger text-white text-xs rounded-full w-5 h-5 flex items-center justify-center">
                  {totalItems > 99 ? '99+' : totalItems}
                </span>
              )}
            </Link>
          </div>
        </div>
      </nav>

      <main className="flex-1">
        <Outlet />
      </main>

      <footer className="bg-white dark:bg-gray-800 border-t border-gray-200 dark:border-gray-700 py-6 mt-8">
        <div className="container-custom text-center text-sm text-gray-500 dark:text-gray-400">
          &copy; {new Date().getFullYear()} E-Shop. Todos los derechos reservados.
        </div>
      </footer>
    </div>
  );
}
