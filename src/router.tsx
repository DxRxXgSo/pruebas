import { BrowserRouter, Routes, Route, Navigate } from 'react-router-dom';
import App from './App';
import ProductsPage from './pages/ProductsPage';
import BasketPage from './pages/BasketPage';

export default function AppRouter() {
  return (
    <BrowserRouter>
      <Routes>
        <Route element={<App />}>
          <Route path="/" element={<Navigate to="/products" replace />} />
          <Route path="/products" element={<ProductsPage />} />
          <Route path="/basket" element={<BasketPage />} />
        </Route>
      </Routes>
    </BrowserRouter>
  );
}
