After 001-room-builder spec was made, implement new features.

## 1. Write UI Inspector tool to edit item footprint dimensions in Item Definition. Footprint may be very huge(50 * 80), and footprint may be a complex figure.

## 2. Write UI Inspector tool to edit a grid. As an Item it can be a complex figure and have a huge size. Do not store "isOccupied" flag in SO. It should be a runtime value.

## 3. GridManager should store a list of initial items. Items can be replaced and rotated.