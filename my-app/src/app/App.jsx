import React, { Suspense, lazy } from 'react';
import './App.css';
import { Routes, Route } from 'react-router-dom';
import { AppProviders } from './providers';
import LoadingSpinner from 'shared/ui/LoadingSpinner/LoadingSpinner';

// ✅ Lazy-loading всех страниц
const SignIn = lazy(() => import('pages/auth/sign-in'));
const SignUp = lazy(() => import('pages/auth/sign-up'));
const HomePage = lazy(() => import('pages/home'));
const Profile = lazy(() => import('pages/profile'));
const ProductPage = lazy(() => import('pages/product-details'));
const Cart = lazy(() => import('pages/cart'));
const CheckoutPage = lazy(() => import('pages/checkout'));
const SearchPage = lazy(() => import('pages/search'));
const AdminPanel = lazy(() => import('pages/admin'));

// Каталог
const Computer = lazy(() => import('pages/catalog/computers'));
const Labtop = lazy(() => import('pages/catalog/laptops'));
const Phone = lazy(() => import('pages/catalog/phones'));
const Speaker = lazy(() => import('pages/catalog/speakers'));
const Television = lazy(() => import('pages/catalog/televisions'));

function App() {
    return (
        <div className="App">
            <AppProviders>
                <Suspense fallback={<LoadingSpinner text="Загрузка страницы..." />}>
                    <Routes>
                        <Route path="/" element={<SignIn />} />
                        <Route path="/SignUp" element={<SignUp />} />
                        <Route path="/SignIn" element={<SignIn />} />
                        <Route path="/HomePage" element={<HomePage />} />
                        <Route path="/Profile" element={<Profile />} />
                        <Route path="/Television" element={<Television />} />
                        <Route path="/Labtop" element={<Labtop />} />
                        <Route path="/Computer" element={<Computer />} />
                        <Route path="/Speaker" element={<Speaker />} />
                        <Route path="/Phone" element={<Phone />} />
                        <Route path="/product/:id" element={<ProductPage />} />
                        <Route path="/Cart" element={<Cart />} />
                        <Route path="/search" element={<SearchPage />} />
                        <Route path="/checkout" element={<CheckoutPage />} />
                        <Route path="/Admin" element={<AdminPanel />} />
                    </Routes>
                </Suspense>
            </AppProviders>
        </div>
    );
}

export default App;