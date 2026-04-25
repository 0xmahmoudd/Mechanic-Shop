-- User roles
CREATE TYPE user_role AS ENUM (
    'Manager',
    'Customer',
    'Employee'
);

-- Work order states
CREATE TYPE workorder_state AS ENUM (
    'Scheduled',
    'In_Progress',
    'Completed',
    'Cancelled'
);

-- Invoice payment status
CREATE TYPE payment_status AS ENUM (
    'Pending',
    'Paid',
    'Cancelled'
);

--------------------------------------------------------------------------------

CREATE TABLE users (
    id              SERIAL PRIMARY KEY,
    first_name      VARCHAR(50)  NOT NULL,
    last_name       VARCHAR(50)  NOT NULL,
    username        VARCHAR(50)  UNIQUE,
    email           VARCHAR(100) UNIQUE NOT NULL,
    password_hash   VARCHAR(255) NOT NULL,
    role            user_role    NOT NULL,

    is_deleted      BOOLEAN      NOT NULL DEFAULT FALSE,
    deleted_at      TIMESTAMP,
    created_at      TIMESTAMP    NOT NULL DEFAULT NOW(),
    updated_at      TIMESTAMP    NOT NULL DEFAULT NOW()
);

-------------------------------------------------------------------------------

CREATE TABLE refresh_tokens (
    id          SERIAL PRIMARY KEY,
    user_id     INT NOT NULL REFERENCES users(id) ON DELETE CASCADE,
    token       VARCHAR(500) NOT NULL,
    expires_at  TIMESTAMP    NOT NULL,
    created_at  TIMESTAMP    NOT NULL DEFAULT NOW()
);

-------------------------------------------------------------------------------

CREATE TABLE customer (
    id          SERIAL PRIMARY KEY,
    user_id     INT NOT NULL UNIQUE REFERENCES users(id),
    phone_number VARCHAR(20),

    is_deleted  BOOLEAN   NOT NULL DEFAULT FALSE,
    deleted_at  TIMESTAMP,
    created_at  TIMESTAMP NOT NULL DEFAULT NOW(),
    updated_at  TIMESTAMP NOT NULL DEFAULT NOW()
);

-------------------------------------------------------------------------------

CREATE TABLE vehicle (
    id              SERIAL PRIMARY KEY,
    customer_id     INT NOT NULL REFERENCES customer(id),

    make            VARCHAR(50) NOT NULL,
    model           VARCHAR(50) NOT NULL,
    year            INT NOT NULL CHECK (year >= 1950),

    license_plate   VARCHAR(20) NOT NULL,
    vin             VARCHAR(50),

    is_deleted      BOOLEAN   NOT NULL DEFAULT FALSE,
    deleted_at      TIMESTAMP,
    created_at      TIMESTAMP NOT NULL DEFAULT NOW(),
    updated_at      TIMESTAMP NOT NULL DEFAULT NOW()
);

-------------------------------------------------------------------------------

CREATE UNIQUE INDEX ux_vehicle_license_plate_ci
ON vehicle (UPPER(license_plate));


-------------------------------------------------------------------------------

CREATE TABLE employee (
    id                  SERIAL PRIMARY KEY,
    user_id             INT NOT NULL UNIQUE REFERENCES users(id),
    is_active            BOOLEAN NOT NULL DEFAULT TRUE,
    title               VARCHAR(50),
    work_hours_per_day  INT NOT NULL DEFAULT 8,
    employment_start_date DATE NOT NULL,

    is_deleted          BOOLEAN   NOT NULL DEFAULT FALSE,
    deleted_at          TIMESTAMP,
    created_at          TIMESTAMP NOT NULL DEFAULT NOW(),
    updated_at          TIMESTAMP NOT NULL DEFAULT NOW()
);


-------------------------------------------------------------------------------

CREATE TABLE employee_salary_history (
    id              SERIAL PRIMARY KEY,
    employee_id     INT NOT NULL REFERENCES employee(id),
    hourly_rate     NUMERIC(10,2) NOT NULL,
    effective_from  DATE NOT NULL,
    effective_to    DATE,
    created_at      TIMESTAMP NOT NULL DEFAULT NOW()
);


-------------------------------------------------------------------------------
CREATE TABLE work_order (
    id          SERIAL PRIMARY KEY,
    vehicle_id  INT NOT NULL REFERENCES vehicle(id),
    start_at    TIMESTAMP,
    end_at      TIMESTAMP,
    state       workorder_state NOT NULL,

    is_deleted  BOOLEAN   NOT NULL DEFAULT FALSE,
    deleted_at  TIMESTAMP,
    created_at  TIMESTAMP NOT NULL DEFAULT NOW(),
    updated_at  TIMESTAMP NOT NULL DEFAULT NOW()
);


-------------------------------------------------------------------------------

CREATE TABLE work_order_employee (
    work_order_id  INT NOT NULL REFERENCES work_order(id),
    employee_id    INT NOT NULL REFERENCES employee(id),
    hours_worked   NUMERIC(5,2),
    role           VARCHAR(50),

    PRIMARY KEY (work_order_id, employee_id)
);


-------------------------------------------------------------------------------

CREATE TABLE repair_task (
    id                  SERIAL PRIMARY KEY,
    name                VARCHAR(100) NOT NULL UNIQUE,
    estimated_duration  INTERVAL,
    default_labor_cost  NUMERIC(10,2) NOT NULL,

    is_deleted          BOOLEAN   NOT NULL DEFAULT FALSE,
    deleted_at          TIMESTAMP,
    created_at          TIMESTAMP NOT NULL DEFAULT NOW(),
    updated_at          TIMESTAMP NOT NULL DEFAULT NOW()
);

-------------------------------------------------------------------------------

CREATE TABLE part (
    id              SERIAL PRIMARY KEY,
    name            VARCHAR(100) NOT NULL,
    description     VARCHAR(255),
    current_cost    NUMERIC(10,2) NOT NULL,
    stock_quantity  INT NOT NULL DEFAULT 0,
    category        VARCHAR(50),
    supplier        VARCHAR(100),

    is_deleted      BOOLEAN   NOT NULL DEFAULT FALSE,
    deleted_at      TIMESTAMP,
    created_at      TIMESTAMP NOT NULL DEFAULT NOW(),
    updated_at      TIMESTAMP NOT NULL DEFAULT NOW()
);


-------------------------------------------------------------------------------

CREATE TABLE part_price_history (
    id              SERIAL PRIMARY KEY,
    part_id         INT NOT NULL REFERENCES part(id),
    unit_cost       NUMERIC(10,2) NOT NULL,
    effective_from  TIMESTAMP NOT NULL,
    effective_to    TIMESTAMP,
    created_at      TIMESTAMP NOT NULL DEFAULT NOW()
);


-------------------------------------------------------------------------------
CREATE TABLE repair_task_part (
    repair_task_id  INT NOT NULL REFERENCES repair_task(id),
    part_id         INT NOT NULL REFERENCES part(id),
    PRIMARY KEY (repair_task_id, part_id)
);


-------------------------------------------------------------------------------

CREATE TABLE work_order_repair_task (
    work_order_id      INT NOT NULL REFERENCES work_order(id),
    repair_task_id     INT NOT NULL REFERENCES repair_task(id),
    quantity           NUMERIC(5,2) NOT NULL DEFAULT 1,
    labor_cost_at_use  NUMERIC(10,2) NOT NULL,
    note               VARCHAR(255),

    PRIMARY KEY (work_order_id, repair_task_id)
);


-------------------------------------------------------------------------------

CREATE TABLE work_order_part (
    id              SERIAL PRIMARY KEY,
    work_order_id   INT NOT NULL REFERENCES work_order(id),
    part_id         INT NOT NULL REFERENCES part(id),
    quantity_used   NUMERIC(10,2) NOT NULL,
    unit_price_at_use NUMERIC(10,2) NOT NULL,
    created_at      TIMESTAMP NOT NULL DEFAULT NOW()
);



-------------------------------------------------------------------------------

CREATE TABLE invoice (
    id              SERIAL PRIMARY KEY,
    work_order_id   INT NOT NULL UNIQUE REFERENCES work_order(id),

    subtotal        NUMERIC(12,2) NOT NULL,
    discount        NUMERIC(12,2) NOT NULL DEFAULT 0,
    tax_rate        NUMERIC(5,2)  NOT NULL,
    tax_amount      NUMERIC(12,2) NOT NULL,
    total_amount   NUMERIC(12,2) NOT NULL,

    payment_status payment_status NOT NULL DEFAULT 'Pending',

    issued_at       TIMESTAMP NOT NULL,
    is_deleted      BOOLEAN   NOT NULL DEFAULT FALSE,
    deleted_at      TIMESTAMP,
    created_at      TIMESTAMP NOT NULL DEFAULT NOW()
);


-------------------------------------------------------------------------------



CREATE OR REPLACE FUNCTION set_updated_at()
RETURNS TRIGGER AS $$
BEGIN
    NEW.updated_at = NOW();
    RETURN NEW;
END;
$$ LANGUAGE plpgsql;


--------------------------------------------------------------


CREATE TRIGGER trg_users_updated_at
BEFORE UPDATE ON users
FOR EACH ROW
EXECUTE FUNCTION set_updated_at();

-------------------------------------------------------------

CREATE TRIGGER trg_customer_updated_at
BEFORE UPDATE ON customer
FOR EACH ROW
EXECUTE FUNCTION set_updated_at();
-------------------------------------------------------------


CREATE TRIGGER trg_vehicle_updated_at
BEFORE UPDATE ON vehicle
FOR EACH ROW
EXECUTE FUNCTION set_updated_at();
-------------------------------------------------------------



CREATE TRIGGER trg_employee_updated_at
BEFORE UPDATE ON employee
FOR EACH ROW
EXECUTE FUNCTION set_updated_at();
-------------------------------------------------------------



CREATE TRIGGER trg_work_order_updated_at
BEFORE UPDATE ON work_order
FOR EACH ROW
EXECUTE FUNCTION set_updated_at();
-------------------------------------------------------------



CREATE TRIGGER trg_repair_task_updated_at
BEFORE UPDATE ON repair_task
FOR EACH ROW
EXECUTE FUNCTION set_updated_at();

-------------------------------------------------------------

CREATE TRIGGER trg_part_updated_at
BEFORE UPDATE ON part
FOR EACH ROW
EXECUTE FUNCTION set_updated_at();


