-- PostgreSQL Database Setup Script
-- This script creates the database and populates it with sample weather summaries

-- Create the database (run this as postgres superuser or database owner)
-- Note: You may need to create this manually if you don't have superuser privileges
-- CREATE DATABASE weatherdb;

-- Connect to the weatherdb database before running the rest of this script

-- Create the summaries table (EF Core migrations will handle this, but here's the manual SQL)
CREATE TABLE IF NOT EXISTS summaries (
    id SERIAL PRIMARY KEY,
    name VARCHAR(100) NOT NULL
);

-- Insert sample weather summaries
INSERT INTO summaries (name) VALUES
    ('Freezing'),
    ('Bracing'),
    ('Chilly'),
    ('Cool'),
    ('Mild'),
    ('Warm'),
    ('Balmy'),
    ('Hot'),
    ('Sweltering'),
    ('Scorching'),
    ('Rainy'),
    ('Cloudy'),
    ('Sunny'),
    ('Windy'),
    ('Foggy')
ON CONFLICT DO NOTHING;

-- Verify the data was inserted
SELECT * FROM summaries;

