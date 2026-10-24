
const Configuration = {
    Auth: {
        SignIn: '/api/Auth/signin',
        SignUp: '/api/Auth/signup',
        ChangePassword: '/api/Auth/change-password',
    },

    Cart: {
        Get: '/api/Cart',
        Clear: '/api/Cart',
        AddItem: '/api/Cart/items',
        UpdateItem: '/api/Cart/items',      // + /{productId}
        RemoveItem: '/api/Cart/items',      // + /{productId}
    },

    Products: {
        GetAll: '/api/Products',
        GetById: '/api/Products',           // + /{id}
        Search: '/api/Products/search',
        ByCategory: '/api/Products/category', // + /{categoryId}
        Paged: '/api/Products/paged',       // ?categoryId=&sort=&minPrice=&maxPrice=&inStockOnly=&page=&pageSize=
        Update: '/api/Products',            // PUT /{id}
        Stock: '/api/Products',             // PUT /{id}/stock
        Activate: '/api/Products',          // POST /{id}/activate
        Deactivate: '/api/Products',        // DELETE /{id}
    },

    Wishlist: {
        Get: '/api/Wishlist',
        Ids: '/api/Wishlist/ids',
        Item: '/api/Wishlist',              // PUT/DELETE /{productId}
    },

    Categories: {
        GetAll: '/api/Categories',
    },

    Brands: {
        GetAll: '/api/Brands',
    },

    Types: {
        GetAll: '/api/Types',
    },

    Orders: {
        Create: '/api/Orders',
        GetMy: '/api/Orders',
        GetById: '/api/Orders',             // + /{id}
        UpdateStatus: '/api/Orders',        // + /{id}/status
    },

    Admin: {
        Stats: '/api/Admin/stats',
        Users: '/api/Admin/users',
        UpdateRole: '/api/Admin/users',     // + /{id}/role
        DeleteUser: '/api/Admin/users',     // + /{id}
        Orders: '/api/Admin/orders',        // ?status=&page=&pageSize=
    },

    Reviews: {
        Create: '/api/Reviews',
        ByProduct: '/api/Reviews/product',  // + /{productId}
        Update: '/api/Reviews',             // + /{id}
        Delete: '/api/Reviews',             // + /{id}
        Pending: '/api/Reviews/pending',
        Moderate: '/api/Reviews',           // PUT /{id}/approve | /{id}/reject
    },

    ProductAttributes: {
        ByProduct: '/api/ProductAttributes/product',
    }
};

export default Configuration;