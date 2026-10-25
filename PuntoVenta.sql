-- =====================================================================
--  Punto de Venta - Script de base de datos (MySQL 8.x)
--  Ejecutar completo en MySQL Workbench o con: mysql -u root -p < PuntoVenta.sql
-- =====================================================================

DROP DATABASE IF EXISTS punto_venta;
CREATE DATABASE punto_venta CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci;
USE punto_venta;

-- ---------------------------------------------------------------------
--  Catalogos
-- ---------------------------------------------------------------------
CREATE TABLE puestos (
    id_puesto   INT AUTO_INCREMENT PRIMARY KEY,
    puesto      VARCHAR(60)  NOT NULL,
    descripcion VARCHAR(150) NULL,
    CONSTRAINT uq_puestos_puesto UNIQUE (puesto)
) ENGINE=InnoDB;

CREATE TABLE empleados (
    id_empleado      INT AUTO_INCREMENT PRIMARY KEY,
    nombres          VARCHAR(60)  NOT NULL,
    apellidos        VARCHAR(60)  NOT NULL,
    direccion        VARCHAR(150) NULL,
    telefono         VARCHAR(20)  NULL,
    dpi              VARCHAR(20)  NOT NULL,
    correo           VARCHAR(100) NULL,
    fecha_nacimiento DATE         NULL,
    id_puesto        INT          NOT NULL,
    fecha_inicio_labores DATE     NULL,
    fecha_ingreso    DATETIME     NOT NULL DEFAULT CURRENT_TIMESTAMP,
    CONSTRAINT uq_empleados_dpi UNIQUE (dpi),
    CONSTRAINT fk_empleados_puesto FOREIGN KEY (id_puesto) REFERENCES puestos (id_puesto)
) ENGINE=InnoDB;

CREATE TABLE clientes (
    id_cliente    INT AUTO_INCREMENT PRIMARY KEY,
    nombres       VARCHAR(60)  NOT NULL,
    apellidos     VARCHAR(60)  NOT NULL,
    nit           VARCHAR(15)  NULL,
    genero        TINYINT(1)   NULL COMMENT '1 = masculino, 0 = femenino',
    telefono      VARCHAR(20)  NULL,
    correo        VARCHAR(100) NULL,
    direccion     VARCHAR(150) NULL,
    fecha_ingreso DATETIME     NOT NULL DEFAULT CURRENT_TIMESTAMP,
    CONSTRAINT uq_clientes_nit UNIQUE (nit)
) ENGINE=InnoDB;

CREATE TABLE proveedores (
    id_proveedor INT AUTO_INCREMENT PRIMARY KEY,
    proveedor    VARCHAR(80)  NOT NULL,
    nit          VARCHAR(15)  NOT NULL,
    direccion    VARCHAR(150) NULL,
    telefono     VARCHAR(20)  NULL,
    correo       VARCHAR(100) NULL,
    CONSTRAINT uq_proveedores_nit UNIQUE (nit)
) ENGINE=InnoDB;

CREATE TABLE marcas (
    id_marca INT AUTO_INCREMENT PRIMARY KEY,
    marca    VARCHAR(60) NOT NULL,
    CONSTRAINT uq_marcas_marca UNIQUE (marca)
) ENGINE=InnoDB;

CREATE TABLE productos (
    id_producto   INT AUTO_INCREMENT PRIMARY KEY,
    producto      VARCHAR(80)   NOT NULL,
    id_marca      INT           NOT NULL,
    descripcion   VARCHAR(255)  NULL,
    imagen        VARCHAR(255)  NULL COMMENT 'Ruta relativa, ej: /images/productos/monitor.jpg',
    precio_costo  DECIMAL(10,2) NOT NULL DEFAULT 0,
    precio_venta  DECIMAL(10,2) NOT NULL DEFAULT 0,
    existencia    INT           NOT NULL DEFAULT 0,
    fecha_ingreso DATETIME      NOT NULL DEFAULT CURRENT_TIMESTAMP,
    CONSTRAINT fk_productos_marca FOREIGN KEY (id_marca) REFERENCES marcas (id_marca),
    CONSTRAINT ck_productos_existencia CHECK (existencia >= 0)
) ENGINE=InnoDB;

-- ---------------------------------------------------------------------
--  Compras (maestro / detalle)
-- ---------------------------------------------------------------------
CREATE TABLE compras (
    id_compra       INT AUTO_INCREMENT PRIMARY KEY,
    no_orden_compra INT           NOT NULL,
    id_proveedor    INT           NOT NULL,
    fecha_orden     DATE          NOT NULL,
    total           DECIMAL(12,2) NOT NULL DEFAULT 0,
    fecha_ingreso   DATETIME      NOT NULL DEFAULT CURRENT_TIMESTAMP,
    CONSTRAINT uq_compras_orden UNIQUE (no_orden_compra),
    CONSTRAINT fk_compras_proveedor FOREIGN KEY (id_proveedor) REFERENCES proveedores (id_proveedor)
) ENGINE=InnoDB;

CREATE TABLE compras_detalle (
    id_compra_detalle INT AUTO_INCREMENT PRIMARY KEY,
    id_compra         INT           NOT NULL,
    id_producto       INT           NOT NULL,
    cantidad          INT           NOT NULL,
    precio_costo_unitario DECIMAL(10,2) NOT NULL,
    CONSTRAINT fk_cd_compra   FOREIGN KEY (id_compra)   REFERENCES compras (id_compra) ON DELETE CASCADE,
    CONSTRAINT fk_cd_producto FOREIGN KEY (id_producto) REFERENCES productos (id_producto),
    CONSTRAINT ck_cd_cantidad CHECK (cantidad > 0),
    CONSTRAINT ck_cd_precio   CHECK (precio_costo_unitario > 0)
) ENGINE=InnoDB;

-- ---------------------------------------------------------------------
--  Ventas (maestro / detalle)
-- ---------------------------------------------------------------------
CREATE TABLE ventas (
    id_venta      INT AUTO_INCREMENT PRIMARY KEY,
    no_factura    INT           NOT NULL,
    serie         VARCHAR(5)    NOT NULL DEFAULT 'A',
    fecha_factura DATE          NOT NULL,
    id_cliente    INT           NOT NULL,
    id_empleado   INT           NOT NULL,
    total         DECIMAL(12,2) NOT NULL DEFAULT 0,
    fecha_ingreso DATETIME      NOT NULL DEFAULT CURRENT_TIMESTAMP,
    CONSTRAINT uq_ventas_factura UNIQUE (serie, no_factura),
    CONSTRAINT fk_ventas_cliente  FOREIGN KEY (id_cliente)  REFERENCES clientes (id_cliente),
    CONSTRAINT fk_ventas_empleado FOREIGN KEY (id_empleado) REFERENCES empleados (id_empleado)
) ENGINE=InnoDB;

CREATE TABLE ventas_detalle (
    id_venta_detalle INT AUTO_INCREMENT PRIMARY KEY,
    id_venta         INT           NOT NULL,
    id_producto      INT           NOT NULL,
    cantidad         INT           NOT NULL,
    precio_unitario  DECIMAL(10,2) NOT NULL,
    CONSTRAINT fk_vd_venta    FOREIGN KEY (id_venta)    REFERENCES ventas (id_venta) ON DELETE CASCADE,
    CONSTRAINT fk_vd_producto FOREIGN KEY (id_producto) REFERENCES productos (id_producto),
    CONSTRAINT ck_vd_cantidad CHECK (cantidad > 0),
    CONSTRAINT ck_vd_precio   CHECK (precio_unitario > 0)
) ENGINE=InnoDB;

-- ---------------------------------------------------------------------
--  Seguridad: roles, usuarios
-- ---------------------------------------------------------------------
CREATE TABLE roles (
    id_rol INT AUTO_INCREMENT PRIMARY KEY,
    rol    VARCHAR(40) NOT NULL,
    CONSTRAINT uq_roles_rol UNIQUE (rol)
) ENGINE=InnoDB;

CREATE TABLE usuarios (
    id_usuario      INT AUTO_INCREMENT PRIMARY KEY,
    usuario         VARCHAR(40)  NOT NULL,
    password_hash   VARCHAR(100) NOT NULL COMMENT 'Hash BCrypt',
    nombre_completo VARCHAR(100) NOT NULL,
    id_rol          INT          NOT NULL,
    activo          TINYINT(1)   NOT NULL DEFAULT 1,
    fecha_ingreso   DATETIME     NOT NULL DEFAULT CURRENT_TIMESTAMP,
    CONSTRAINT uq_usuarios_usuario UNIQUE (usuario),
    CONSTRAINT fk_usuarios_rol FOREIGN KEY (id_rol) REFERENCES roles (id_rol)
) ENGINE=InnoDB;

-- ---------------------------------------------------------------------
--  Menu dinamico (estructura padre-hijo)
-- ---------------------------------------------------------------------
CREATE TABLE menu (
    id_menu  INT AUTO_INCREMENT PRIMARY KEY,
    nombre   VARCHAR(60)  NOT NULL,
    ruta     VARCHAR(100) NULL COMMENT 'Ruta de Blazor, ej: /marcas',
    icono    VARCHAR(40)  NULL COMMENT 'Clase de Bootstrap Icons, ej: bi-box-seam',
    id_padre INT          NULL,
    orden    INT          NOT NULL DEFAULT 0,
    activo   TINYINT(1)   NOT NULL DEFAULT 1,
    CONSTRAINT fk_menu_padre FOREIGN KEY (id_padre) REFERENCES menu (id_menu) ON DELETE CASCADE
) ENGINE=InnoDB;

-- =====================================================================
--  DATOS INICIALES
--  Nota: el usuario "admin" lo crea la API al iniciar (contrasena: Admin123*)
--  para que el hash BCrypt se genere con la libreria y no a mano.
-- =====================================================================
INSERT INTO roles (rol) VALUES ('Administrador'), ('Vendedor');

-- Menu: 1. Productos > Marcas | 2. Ventas > Clientes, Empleados > Puestos
--       3. Compras > Proveedores | 4. Reportes
INSERT INTO menu (id_menu, nombre, ruta, icono, id_padre, orden) VALUES
(1,  'Productos',   '/productos',   'bi-box-seam',     NULL, 1),
(2,  'Marcas',      '/marcas',      'bi-tags',         1,    1),
(3,  'Ventas',      '/ventas',      'bi-cart-check',   NULL, 2),
(4,  'Clientes',    '/clientes',    'bi-people',       3,    1),
(5,  'Empleados',   '/empleados',   'bi-person-badge', 3,    2),
(6,  'Puestos',     '/puestos',     'bi-briefcase',    5,    1),
(7,  'Compras',     '/compras',     'bi-bag-plus',     NULL, 3),
(8,  'Proveedores', '/proveedores', 'bi-truck',        7,    1),
(9,  'Reportes',    NULL,           'bi-bar-chart',    NULL, 4),
(10, 'Reporte de productos',   '/reportes/productos',   'bi-file-earmark-text', 9, 1),
(11, 'Reporte de existencias', '/reportes/existencias', 'bi-file-earmark-text', 9, 2),
(12, 'Reporte de compras',     '/reportes/compras',     'bi-file-earmark-text', 9, 3),
(13, 'Reporte de ventas',      '/reportes/ventas',      'bi-file-earmark-text', 9, 4);

-- Datos de ejemplo
INSERT INTO puestos (puesto, descripcion) VALUES
('Gerente', 'Administra la tienda'),
('Vendedor', 'Atiende y factura a clientes'),
('Bodeguero', 'Control de inventario');

INSERT INTO empleados (nombres, apellidos, direccion, telefono, dpi, correo, fecha_nacimiento, id_puesto, fecha_inicio_labores) VALUES
('Ana', 'Morales', 'Zona 1, Villa Nueva', '55501234', '2500123450101', 'ana@tienda.com', '1995-04-12', 2, '2024-01-15'),
('Luis', 'Perez', 'Zona 3, Villa Nueva', '55505678', '2500123450102', 'luis@tienda.com', '1990-09-30', 1, '2023-06-01');

INSERT INTO clientes (nombres, apellidos, nit, genero, telefono, correo, direccion) VALUES
('Consumidor', 'Final', 'CF', 1, NULL, NULL, 'Ciudad'),
('Maria', 'Lopez', '1234567-8', 0, '55509999', 'maria@correo.com', 'Zona 5');

INSERT INTO proveedores (proveedor, nit, direccion, telefono, correo) VALUES
('Distribuidora Tecno S.A.', '9876543-2', 'Zona 12, Guatemala', '22334455', 'ventas@tecno.com');

INSERT INTO marcas (marca) VALUES ('Samsung'), ('Logitech'), ('HP');

INSERT INTO productos (producto, id_marca, descripcion, imagen, precio_costo, precio_venta, existencia) VALUES
('Monitor 24 pulgadas', 1, 'Monitor Full HD', NULL, 800.00, 1000.00, 10),
('Mouse inalambrico',   2, 'Mouse optico USB', NULL, 80.00,  100.00,  25);
