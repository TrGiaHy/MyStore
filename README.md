========================================
 E-COMMERCE WEBSITE FEATURES
========================================

🛒 CUSTOMER FEATURES
----------------------------------------
- Authentication
  * Register / Login / Logout

- Product Browsing
  * View product list (by category)
  * View product details (name, price, description, images, stock)
  * Search for products

- Shopping Cart
  * Add products to cart
  * Update product quantity
  * Remove products from cart

- Checkout
  * Enter shipping address
  * Select payment method (Cash on Delivery)

- Orders
  * View order history
  * Track order status (Pending, Shipping, Completed, Canceled)


🏪 SELLER / ADMIN FEATURES
----------------------------------------
- Product Management
  * Add / Edit / Delete products
  * Upload product images
  * Manage stock levels

- Order Management
  * View all customer orders
  * Update order status (Pending → Shipping → Completed / Canceled)

- Customer Management
  * View customer list
  * Enable / Disable customer accounts if needed



---------------------------------------------------------------------------------
## 🔑 Identity (Microsoft Identity mặc định)

* **AspNetUsers**

  * Id (PK)
  * UserName
  * Email
  * PasswordHash
  * … (các trường mặc định của Identity)
  * `FullName` (thêm)
  * `Address` (thêm)
  * `IsActive` (bool – để Enable/Disable account)

* **AspNetRoles**

  * Id (PK)
  * Name (e.g., `Customer`, `Seller`)

* **AspNetUserRoles** (mapping N-N giữa User và Role)

---

## 🛍️ Product Management

* **Categories**

  * CategoryId (PK)
  * Name
  * Description

* **Products**

  * ProductId (PK)
  * Name
  * Description
  * Price (decimal)
  * StockQuantity (int)
  * CategoryId (FK → Categories)
  * CreatedDate

* **ProductImages**

  * ImageId (PK)
  * ProductId (FK → Products)
  * ImageUrl

---

## 🛒 Shopping Cart

* **ShoppingCarts**

  * CartId (PK)
  * UserId (FK → AspNetUsers)

* **CartItems**

  * CartItemId (PK)
  * CartId (FK → ShoppingCarts)
  * ProductId (FK → Products)
  * Quantity

---

## 📦 Orders

* **Orders**

  * OrderId (PK)
  * UserId (FK → AspNetUsers)
  * OrderDate
  * ShippingAddress
  * PaymentMethod (e.g., "COD")
  * Status (Pending, Shipping, Completed, Canceled)

* **OrderItems**

  * OrderItemId (PK)
  * OrderId (FK → Orders)
  * ProductId (FK → Products)
  * Quantity
  * UnitPrice

---

## 🔗 Quan hệ chính

* 1 Category → nhiều Product
* 1 Product → nhiều ProductImage
* 1 User → 1 ShoppingCart → nhiều CartItem
* 1 Order → nhiều OrderItem
* 1 User → nhiều Order

---

