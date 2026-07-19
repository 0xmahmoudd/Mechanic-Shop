DO $$
DECLARE
    i INT;
BEGIN
    FOR i IN 1..10000 LOOP
        INSERT INTO work_order (vehicle_id, start_at, state)
        VALUES ((SELECT id FROM vehicle ORDER BY RANDOM() LIMIT 1), NOW() - (random() * interval '30 days'), 'Completed');
    END LOOP;
END $$;
