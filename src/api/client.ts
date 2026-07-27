import axios from 'axios';

const API_URL = import.meta.env.VITE_CATALOG_API_URL || 'http://localhost:8080';
const BASKET_API_URL = import.meta.env.VITE_BASKET_API_URL || 'http://localhost:8082';

export const catalogClient = axios.create({ baseURL: `${API_URL}/api` });
export const basketClient = axios.create({ baseURL: `${BASKET_API_URL}/api` });
