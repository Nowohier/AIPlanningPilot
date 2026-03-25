# Codebase Quick Reference

## Entity Counts

| Tier | Count | Example | Key Characteristics |
|------|------:|---------|---------------------|
| Simple | ~55 | AdditionalRevenueType | List + Form, few fields |
| Medium | ~35 | Supplier, Customer | Tabs, multiple FKs |
| Complex | ~15 | InvoiceHeader | State machines |
| Very Complex | ~8 | ConditionSheet | Fully custom |
| Backend-only | ~218 | Junction tables | No frontend |
| **Total** | **331** | | 113 with frontend |

## Key Numbers

| Metric | Count |
|--------|------:|
| Domain-UI libraries | 15 |
| Frontend layout configs | 225 |
| NgRx feature stores | 32 |

## Migration Order

1. Static (1 dep) -> 2. Taxonomy (2) -> 3. Common (2) -> 4. Identity (5)
