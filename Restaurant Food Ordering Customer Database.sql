-- =============================================
-- TABLE 2: Customers
-- =============================================
CREATE TABLE IF NOT EXISTS `Customers` (
    `CustomerId` INT NOT NULL AUTO_INCREMENT,
    `FullName` VARCHAR(100) NOT NULL,
    `Email` VARCHAR(100) NOT NULL,
    `Phone` VARCHAR(15) NOT NULL,
    `Password` VARCHAR(255) NOT NULL,
    `Address` VARCHAR(500) NULL,
    `CreatedDate` DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    `IsActive` TINYINT(1) NOT NULL DEFAULT 1,
    PRIMARY KEY (`CustomerId`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- =============================================
-- TABLE 3: Categories
-- =============================================
CREATE TABLE IF NOT EXISTS `Categories` (
    `CategoryId` INT NOT NULL AUTO_INCREMENT,
    `CategoryName` VARCHAR(100) NOT NULL,
    `Description` VARCHAR(500) NULL,
    `ImageUrl` VARCHAR(500) NULL,
    `IsActive` TINYINT(1) NOT NULL DEFAULT 1,
    `IsVeg` TINYINT(1) NOT NULL DEFAULT 1,
    `CreatedDate` DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    PRIMARY KEY (`CategoryId`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- =============================================
-- TABLE 4: FoodItems
-- =============================================
CREATE TABLE IF NOT EXISTS `FoodItems` (
    `FoodId` INT NOT NULL AUTO_INCREMENT,
    `FoodName` VARCHAR(200) NOT NULL,
    `Description` VARCHAR(1000) NULL,
    `Price` DECIMAL(10,2) NOT NULL,
    `ImageUrl` VARCHAR(500) NULL,
    `CategoryId` INT NOT NULL,
    `IsAvailable` TINYINT(1) NOT NULL DEFAULT 1,
    `IsVeg` TINYINT(1) NOT NULL DEFAULT 1,
    `CreatedDate` DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    PRIMARY KEY (`FoodId`),
    CONSTRAINT `FK_FoodItems_Categories` FOREIGN KEY (`CategoryId`)
        REFERENCES `Categories` (`CategoryId`) ON DELETE RESTRICT
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- =============================================
-- TABLE 5: Coupons
-- =============================================
CREATE TABLE IF NOT EXISTS `Coupons` (
    `CouponId` INT NOT NULL AUTO_INCREMENT,
    `CouponCode` VARCHAR(20) NOT NULL,
    `Description` VARCHAR(200) NOT NULL,
    `DiscountType` VARCHAR(20) NOT NULL DEFAULT 'Percentage',
    `DiscountValue` DECIMAL(10,2) NOT NULL,
    `MaxDiscountAmount` DECIMAL(10,2) NULL,
    `MinimumOrderAmount` DECIMAL(10,2) NOT NULL DEFAULT 0.00,
    `StartDate` DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    `ExpiryDate` DATETIME NOT NULL,
    `IsActive` TINYINT(1) NOT NULL DEFAULT 1,
    `UsageLimit` INT NOT NULL DEFAULT 1,
    `CreatedDate` DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    PRIMARY KEY (`CouponId`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- =============================================
-- TABLE 6: Orders
-- =============================================
CREATE TABLE IF NOT EXISTS `Orders` (
    `OrderId` INT NOT NULL AUTO_INCREMENT,
    `CustomerId` INT NOT NULL,
    `TotalAmount` DECIMAL(10,2) NOT NULL,
    `DiscountAmount` DECIMAL(10,2) NULL,
    `FinalAmount` DECIMAL(10,2) NULL,
    `CouponCode` VARCHAR(20) NULL,
    `CouponId` INT NULL,
    `Status` VARCHAR(50) NOT NULL DEFAULT 'Pending',
    `DeliveryAddress` VARCHAR(500) NOT NULL,
    `ContactPhone` VARCHAR(15) NULL,
    `SpecialInstructions` VARCHAR(1000) NULL,
    `PaymentMethod` VARCHAR(50) NULL,
    `PaymentStatus` VARCHAR(20) NULL,
    `TransactionId` VARCHAR(100) NULL,
    `OrderDate` DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    `DeliveryDate` DATETIME NULL,
    `DeliveryUserId` INT NULL,
    PRIMARY KEY (`OrderId`),
    CONSTRAINT `FK_Orders_Customers` FOREIGN KEY (`CustomerId`)
        REFERENCES `Customers` (`CustomerId`) ON DELETE RESTRICT,
    CONSTRAINT `FK_Orders_Coupons` FOREIGN KEY (`CouponId`)
        REFERENCES `Coupons` (`CouponId`) ON DELETE SET NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- =============================================
-- TABLE 7: OrderItems
-- =============================================
CREATE TABLE IF NOT EXISTS `OrderItems` (
    `OrderItemId` INT NOT NULL AUTO_INCREMENT,
    `OrderId` INT NOT NULL,
    `FoodId` INT NOT NULL,
    `Quantity` INT NOT NULL,
    `Price` DECIMAL(10,2) NOT NULL,
    `TotalPrice` DECIMAL(10,2) NOT NULL DEFAULT 0.00,
    PRIMARY KEY (`OrderItemId`),
    CONSTRAINT `FK_OrderItems_Orders` FOREIGN KEY (`OrderId`)
        REFERENCES `Orders` (`OrderId`) ON DELETE CASCADE,
    CONSTRAINT `FK_OrderItems_FoodItems` FOREIGN KEY (`FoodId`)
        REFERENCES `FoodItems` (`FoodId`) ON DELETE RESTRICT
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- =============================================
-- TABLE 8: Reviews
-- =============================================
CREATE TABLE IF NOT EXISTS `Reviews` (
    `ReviewId` INT NOT NULL AUTO_INCREMENT,
    `CustomerId` INT NOT NULL,
    `FoodId` INT NOT NULL,
    `Rating` INT NOT NULL,
    `Comment` VARCHAR(1000) NULL,
    `ReviewDate` DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    `IsApproved` TINYINT(1) NOT NULL DEFAULT 1,
    PRIMARY KEY (`ReviewId`),
    CONSTRAINT `FK_Reviews_Customers` FOREIGN KEY (`CustomerId`)
        REFERENCES `Customers` (`CustomerId`) ON DELETE RESTRICT,
    CONSTRAINT `FK_Reviews_FoodItems` FOREIGN KEY (`FoodId`)
        REFERENCES `FoodItems` (`FoodId`) ON DELETE RESTRICT
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- =============================================
-- TABLE 9: CustomerCoupons
-- =============================================
CREATE TABLE IF NOT EXISTS `CustomerCoupons` (
    `CustomerCouponId` INT NOT NULL AUTO_INCREMENT,
    `CustomerId` INT NOT NULL,
    `CouponId` INT NOT NULL,
    `AssignedDate` DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    `RemainingUsage` INT NOT NULL DEFAULT 1,
    `UsedDate` DATETIME NULL,
    `IsUsed` TINYINT(1) NOT NULL DEFAULT 0,
    `OrderId` INT NULL,
    PRIMARY KEY (`CustomerCouponId`),
    CONSTRAINT `FK_CustomerCoupons_Customers` FOREIGN KEY (`CustomerId`)
        REFERENCES `Customers` (`CustomerId`) ON DELETE RESTRICT,
    CONSTRAINT `FK_CustomerCoupons_Coupons` FOREIGN KEY (`CouponId`)
        REFERENCES `Coupons` (`CouponId`) ON DELETE CASCADE,
    CONSTRAINT `FK_CustomerCoupons_Orders` FOREIGN KEY (`OrderId`)
        REFERENCES `Orders` (`OrderId`) ON DELETE SET NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- =============================================
-- TABLE 10: Combos
-- =============================================
CREATE TABLE IF NOT EXISTS `Combos` (
    `ComboId` INT NOT NULL AUTO_INCREMENT,
    `ComboName` VARCHAR(200) NOT NULL,
    `Description` VARCHAR(1000) NULL,
    `OriginalPrice` DECIMAL(10,2) NOT NULL,
    `ComboPrice` DECIMAL(10,2) NOT NULL,
    `Discount` DECIMAL(5,2) NOT NULL DEFAULT 0.00,
    `ImageUrl` VARCHAR(500) NULL,
    `IsAvailable` TINYINT(1) NOT NULL DEFAULT 1,
    `IsVeg` TINYINT(1) NOT NULL DEFAULT 1,
    `CreatedDate` DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    `ValidUntil` DATETIME NULL,
    PRIMARY KEY (`ComboId`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- =============================================
-- TABLE 11: ComboItems
-- =============================================
CREATE TABLE IF NOT EXISTS `ComboItems` (
    `ComboItemId` INT NOT NULL AUTO_INCREMENT,
    `ComboId` INT NOT NULL,
    `FoodId` INT NOT NULL,
    `Quantity` INT NOT NULL DEFAULT 1,
    PRIMARY KEY (`ComboItemId`),
    CONSTRAINT `FK_ComboItems_Combos` FOREIGN KEY (`ComboId`)
        REFERENCES `Combos` (`ComboId`) ON DELETE CASCADE,
    CONSTRAINT `FK_ComboItems_FoodItems` FOREIGN KEY (`FoodId`)
        REFERENCES `FoodItems` (`FoodId`) ON DELETE RESTRICT
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- =============================================
-- INSERT: Customers
-- =============================================
INSERT INTO `Customers` (`FullName`, `Email`, `Phone`, `Password`, `Address`, `IsActive`) VALUES
('Rahul Sharma', 'rahul@gmail.com', '9876543210', 'password123', '123 MG Road, Ahmedabad, Gujarat', 1),
('Priya Patel', 'priya@gmail.com', '9876543211', 'password123', '456 SG Highway, Ahmedabad, Gujarat', 1),
('Amit Singh', 'amit@gmail.com', '9876543212', 'password123', '789 CG Road, Ahmedabad, Gujarat', 1),
('Neha Desai', 'neha@gmail.com', '9876543213', 'password123', '321 Navrangpura, Ahmedabad, Gujarat', 1),
('Kiran Mehta', 'kiran@gmail.com', '9876543214', 'password123', '654 Satellite, Ahmedabad, Gujarat', 1);

-- =============================================
-- INSERT: Categories
-- =============================================
INSERT INTO `Categories` (`CategoryName`, `Description`, `ImageUrl`, `IsActive`, `IsVeg`) VALUES
('Pizza', 'Delicious Italian pizzas with various toppings', '/images/categories/pizza.jpg', 1, 0),
('Burger', 'Juicy burgers with fresh ingredients', '/images/categories/burger.jpg', 1, 0),
('Biryani', 'Aromatic rice dishes with rich spices', '/images/categories/biryani.jpg', 1, 0),
('Chinese', 'Popular Indo-Chinese cuisine', '/images/categories/chinese.jpg', 1, 0),
('South Indian', 'Traditional South Indian dishes', '/images/categories/south-indian.jpg', 1, 1),
('Desserts', 'Sweet treats and desserts', '/images/categories/desserts.jpg', 1, 1),
('Beverages', 'Refreshing drinks and beverages', '/images/categories/beverages.jpg', 1, 1),
('Thali', 'Complete meal thali combos', '/images/categories/thali.jpg', 1, 0),
('Sandwich', 'Fresh sandwiches and wraps', '/images/categories/sandwich.jpg', 1, 0),
('Pasta', 'Italian pasta and noodle dishes', '/images/categories/pasta.jpg', 1, 0);

-- =============================================
-- INSERT: FoodItems
-- =============================================
INSERT INTO `FoodItems` (`FoodName`, `Description`, `Price`, `ImageUrl`, `CategoryId`, `IsAvailable`, `IsVeg`) VALUES
-- Pizza (CategoryId = 1)
('Margherita Pizza', 'Classic pizza with mozzarella cheese and tomato sauce', 249.00, '/images/food/margherita.jpg', 1, 1, 1),
('Pepperoni Pizza', 'Topped with pepperoni slices and mozzarella', 349.00, '/images/food/pepperoni.jpg', 1, 1, 0),
('Veggie Supreme Pizza', 'Loaded with fresh vegetables and cheese', 299.00, '/images/food/veggie-supreme.jpg', 1, 1, 1),
('Paneer Tikka Pizza', 'Indian style paneer tikka on pizza base', 329.00, '/images/food/paneer-pizza.jpg', 1, 1, 1),
-- Burger (CategoryId = 2)
('Classic Veg Burger', 'Crispy veg patty with lettuce and mayo', 149.00, '/images/food/veg-burger.jpg', 2, 1, 1),
('Chicken Burger', 'Grilled chicken patty with cheese', 199.00, '/images/food/chicken-burger.jpg', 2, 1, 0),
('Paneer Burger', 'Spicy paneer patty with special sauce', 179.00, '/images/food/paneer-burger.jpg', 2, 1, 1),
-- Biryani (CategoryId = 3)
('Veg Biryani', 'Fragrant basmati rice with mixed vegetables', 199.00, '/images/food/veg-biryani.jpg', 3, 1, 1),
('Chicken Biryani', 'Hyderabadi style chicken biryani', 299.00, '/images/food/chicken-biryani.jpg', 3, 1, 0),
('Mutton Biryani', 'Rich and flavorful mutton biryani', 399.00, '/images/food/mutton-biryani.jpg', 3, 1, 0),
-- Chinese (CategoryId = 4)
('Veg Manchurian', 'Crispy vegetable balls in manchurian sauce', 179.00, '/images/food/veg-manchurian.jpg', 4, 1, 1),
('Chicken Manchurian', 'Crispy chicken in manchurian gravy', 229.00, '/images/food/chicken-manchurian.jpg', 4, 1, 0),
('Veg Fried Rice', 'Stir-fried rice with vegetables', 159.00, '/images/food/veg-fried-rice.jpg', 4, 1, 1),
('Hakka Noodles', 'Indo-Chinese style stir-fried noodles', 169.00, '/images/food/hakka-noodles.jpg', 4, 1, 1),
-- South Indian (CategoryId = 5)
('Masala Dosa', 'Crispy dosa with potato masala filling', 129.00, '/images/food/masala-dosa.jpg', 5, 1, 1),
('Idli Sambar', 'Soft idlis served with sambar and chutney', 99.00, '/images/food/idli-sambar.jpg', 5, 1, 1),
('Medu Vada', 'Crispy lentil fritters with chutney', 89.00, '/images/food/medu-vada.jpg', 5, 1, 1),
-- Desserts (CategoryId = 6)
('Gulab Jamun', 'Soft milk dumplings in sugar syrup', 99.00, '/images/food/gulab-jamun.jpg', 6, 1, 1),
('Chocolate Brownie', 'Rich chocolate brownie with ice cream', 149.00, '/images/food/brownie.jpg', 6, 1, 1),
('Rasmalai', 'Soft cottage cheese in sweetened milk', 129.00, '/images/food/rasmalai.jpg', 6, 1, 1),
-- Beverages (CategoryId = 7)
('Mango Lassi', 'Fresh mango yogurt smoothie', 79.00, '/images/food/mango-lassi.jpg', 7, 1, 1),
('Masala Chai', 'Indian spiced tea', 39.00, '/images/food/masala-chai.jpg', 7, 1, 1),
('Cold Coffee', 'Chilled coffee with ice cream', 99.00, '/images/food/cold-coffee.jpg', 7, 1, 1),
('Fresh Lime Soda', 'Refreshing lime soda', 49.00, '/images/food/lime-soda.jpg', 7, 1, 1);

-- =============================================
-- INSERT: Coupons
-- =============================================
INSERT INTO `Coupons` (`CouponCode`, `Description`, `DiscountType`, `DiscountValue`, `MaxDiscountAmount`, `MinimumOrderAmount`, `StartDate`, `ExpiryDate`, `IsActive`, `UsageLimit`) VALUES
('WELCOME50', 'Flat 50% off on first order', 'Percentage', 50.00, 200.00, 299.00, NOW(), DATE_ADD(NOW(), INTERVAL 90 DAY), 1, 1),
('FLAT100', 'Flat Rs.100 off on orders above Rs.500', 'Fixed', 100.00, NULL, 500.00, NOW(), DATE_ADD(NOW(), INTERVAL 60 DAY), 1, 3),
('SAVE20', '20% off on all orders', 'Percentage', 20.00, 150.00, 200.00, NOW(), DATE_ADD(NOW(), INTERVAL 30 DAY), 1, 2),
('TASTY30', '30% off on orders above Rs.400', 'Percentage', 30.00, 250.00, 400.00, NOW(), DATE_ADD(NOW(), INTERVAL 45 DAY), 1, 1),
('FREEDELIVERY', 'Free delivery on orders above Rs.300', 'Fixed', 50.00, NULL, 300.00, NOW(), DATE_ADD(NOW(), INTERVAL 60 DAY), 1, 5);

-- =============================================
-- INSERT: Orders
-- =============================================
INSERT INTO `Orders` (`CustomerId`, `TotalAmount`, `DiscountAmount`, `FinalAmount`, `CouponCode`, `Status`, `DeliveryAddress`, `ContactPhone`, `PaymentMethod`, `PaymentStatus`, `OrderDate`) VALUES
(1, 548.00, 100.00, 448.00, 'FLAT100', 'Delivered', '123 MG Road, Ahmedabad', '9876543210', 'Online', 'Paid', DATE_SUB(NOW(), INTERVAL 5 DAY)),
(2, 349.00, 0.00, 349.00, NULL, 'Delivered', '456 SG Highway, Ahmedabad', '9876543211', 'COD', 'Paid', DATE_SUB(NOW(), INTERVAL 4 DAY)),
(3, 697.00, 150.00, 547.00, 'SAVE20', 'Preparing', '789 CG Road, Ahmedabad', '9876543212', 'Online', 'Paid', DATE_SUB(NOW(), INTERVAL 1 DAY)),
(1, 299.00, 0.00, 299.00, NULL, 'Pending', '123 MG Road, Ahmedabad', '9876543210', 'COD', 'Pending', NOW()),
(4, 428.00, 50.00, 378.00, 'FREEDELIVERY', 'Out for Delivery', '321 Navrangpura, Ahmedabad', '9876543213', 'Online', 'Paid', DATE_SUB(NOW(), INTERVAL 2 HOUR));

-- =============================================
-- INSERT: OrderItems
-- =============================================
INSERT INTO `OrderItems` (`OrderId`, `FoodId`, `Quantity`, `Price`, `TotalPrice`) VALUES
(1, 1, 2, 249.00, 498.00),
(1, 24, 1, 49.00, 49.00),
(2, 2, 1, 349.00, 349.00),
(3, 9, 2, 299.00, 598.00),
(3, 22, 1, 99.00, 99.00),
(4, 8, 1, 199.00, 199.00),
(4, 21, 1, 79.00, 79.00),
(5, 15, 2, 129.00, 258.00),
(5, 17, 1, 89.00, 89.00),
(5, 22, 1, 79.00, 79.00);

-- =============================================
-- INSERT: Reviews
-- =============================================
INSERT INTO `Reviews` (`CustomerId`, `FoodId`, `Rating`, `Comment`, `IsApproved`) VALUES
(1, 1, 5, 'Amazing Margherita Pizza! Best in town.', 1),
(2, 9, 4, 'Chicken Biryani was good, slightly less spicy.', 1),
(3, 15, 5, 'Masala Dosa was crispy and delicious!', 1),
(1, 19, 4, 'Chocolate Brownie was rich and tasty.', 1),
(4, 5, 3, 'Veg Burger was average, expected more crunch.', 1),
(2, 8, 5, 'Best Veg Biryani I have ever had!', 1);

-- =============================================
-- INSERT: CustomerCoupons
-- =============================================
INSERT INTO `CustomerCoupons` (`CustomerId`, `CouponId`, `RemainingUsage`, `IsUsed`) VALUES
(1, 1, 0, 1),
(2, 1, 1, 0),
(3, 2, 2, 0),
(4, 3, 1, 0),
(5, 4, 1, 0),
(1, 5, 4, 0);

-- =============================================
-- INSERT: Combos
-- =============================================
INSERT INTO `Combos` (`ComboName`, `Description`, `OriginalPrice`, `ComboPrice`, `Discount`, `ImageUrl`, `IsAvailable`, `IsVeg`, `ValidUntil`) VALUES
('Pizza Party Combo', '2 Margherita Pizza + 2 Cold Coffee', 696.00, 499.00, 28.30, '/images/combos/pizza-party.jpg', 1, 1, DATE_ADD(NOW(), INTERVAL 30 DAY)),
('Biryani Feast', '1 Chicken Biryani + 1 Veg Biryani + 2 Mango Lassi', 656.00, 499.00, 23.90, '/images/combos/biryani-feast.jpg', 1, 0, DATE_ADD(NOW(), INTERVAL 30 DAY)),
('South Indian Special', '2 Masala Dosa + 2 Idli Sambar + 2 Masala Chai', 534.00, 399.00, 25.30, '/images/combos/south-indian-combo.jpg', 1, 1, DATE_ADD(NOW(), INTERVAL 45 DAY)),
('Burger Mania', '2 Classic Veg Burger + 1 Paneer Burger + 3 Fresh Lime Soda', 624.00, 449.00, 28.00, '/images/combos/burger-mania.jpg', 1, 1, DATE_ADD(NOW(), INTERVAL 30 DAY));

-- =============================================
-- INSERT: ComboItems
-- =============================================
INSERT INTO `ComboItems` (`ComboId`, `FoodId`, `Quantity`) VALUES
(1, 1, 2),   -- 2x Margherita Pizza
(1, 23, 2),  -- 2x Cold Coffee
(2, 9, 1),   -- 1x Chicken Biryani
(2, 8, 1),   -- 1x Veg Biryani
(2, 21, 2),  -- 2x Mango Lassi
(3, 15, 2),  -- 2x Masala Dosa
(3, 16, 2),  -- 2x Idli Sambar
(3, 22, 2),  -- 2x Masala Chai
(4, 5, 2),   -- 2x Classic Veg Burger
(4, 7, 1),   -- 1x Paneer Burger
(4, 24, 3);  -- 3x Fresh Lime Soda

