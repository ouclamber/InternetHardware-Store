import React from 'react';
import './ProductGallery.css';
import { API_URL } from 'shared/api/baseUrl';

const ProductGallery = ({ product, mainImage, images, selectedImage, onImageSelect }) => {
    const fixUrl = (url) => {
        if (!url) return null;
        if (url.startsWith('http')) return url;
        return `${API_URL}${url.startsWith('/') ? '' : '/'}${url}`;
    };

    return (
        <div className="product-gallery">
            <div className="main-image">
                {mainImage ? (
                    <img
                        src={mainImage}
                        alt={product.Name}
                        onError={(e) => {
                            e.target.onerror = null;
                            e.target.src = 'https://via.placeholder.com/400x400?text=No+Image';
                        }}
                    />
                ) : (
                    <div className="no-image-placeholder">
                        <span>Нет изображения</span>
                    </div>
                )}
            </div>

            {images.length > 1 && (
                <div className="image-thumbnails">
                    {images.map((img, index) => {
                        const imgUrl = fixUrl(img.imageUrl || img.ImageUrl);
                        return (
                            <div
                                key={img.id || index}
                                className={`thumbnail ${selectedImage === imgUrl ? 'active' : ''}`}
                                onClick={() => onImageSelect(imgUrl)}
                            >
                                <img
                                    src={imgUrl}
                                    alt={`${product.Name} ${index + 1}`}
                                    onError={(e) => {
                                        e.target.onerror = null;
                                        e.target.style.display = 'none';
                                    }}
                                />
                            </div>
                        );
                    })}
                </div>
            )}
        </div>
    );
};

export default ProductGallery;