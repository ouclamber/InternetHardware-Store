import { API_URL } from 'shared/api/baseUrl';

const pick = (obj, ...keys) => {
    for (const key of keys) {
        if (obj[key] !== undefined && obj[key] !== null) return obj[key];
    }
    return undefined;
};

export const useProductHelpers = () => {
    const getProductImage = (product) => {
        let imageUrl = pick(product, 'ImageUrl', 'imageUrl', 'MainImage', 'mainImage');

        if (!imageUrl) {
            const images = product.Images ?? product.images;
            if (Array.isArray(images) && images.length > 0) {
                const main = images.find(img => img.IsMain ?? img.isMain) ?? images[0];
                imageUrl = pick(main, 'ImageUrl', 'imageUrl');
            }
        }

        if (!imageUrl || imageUrl === 'string') return null;

        if (!imageUrl.startsWith('http')) {
            imageUrl = `${API_URL}${imageUrl.startsWith('/') ? '' : '/'}${imageUrl}`;
        }
        return imageUrl;
    };

    const getBrandName = (product, defaultBrands = {}) => {
        const brandName = pick(product, 'BrandName', 'brandName');
        if (brandName) return brandName;

        const brand = product.Brand ?? product.brand;
        if (brand) {
            const name = pick(brand, 'Name', 'name');
            if (name) return name;
        }

        const name = (pick(product, 'Name', 'name') || '').toLowerCase();
        for (const [key, value] of Object.entries(defaultBrands)) {
            if (name.includes(key)) return value;
        }
        return 'Не указан';
    };

    return { getProductImage, getBrandName };
};