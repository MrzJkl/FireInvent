import 'package:flutter/material.dart';
import 'package:data_table_2/data_table_2.dart';
import '../models/clothing_product_model.dart';

class ClothingProductTable extends StatelessWidget {
  final List<ClothingProductModel> products;
  final void Function(ClothingProductModel) onEdit;
  final void Function(String) onDelete;

  const ClothingProductTable({
    super.key,
    required this.products,
    required this.onEdit,
    required this.onDelete,
  });

  @override
  Widget build(BuildContext context) {
    return DataTable2(
      columnSpacing: 16,
      horizontalMargin: 12,
      columns: const [
        DataColumn(label: Text('Name')),
        DataColumn(label: Text('Manufacturer')),
        DataColumn(label: Text('Type')),
        DataColumn(label: Text('Actions')),
      ],
      rows:
          products.map((product) {
            return DataRow(
              cells: [
                DataCell(Text(product.name)),
                DataCell(Text(product.manufacturer)),
                DataCell(Text(product.type.name)),
                DataCell(
                  Row(
                    children: [
                      IconButton(
                        icon: const Icon(Icons.edit),
                        onPressed: () => onEdit(product),
                        tooltip: 'Edit',
                      ),
                      IconButton(
                        icon: const Icon(Icons.delete),
                        onPressed: () => onDelete(product.id!),
                        tooltip: 'Delete',
                      ),
                    ],
                  ),
                ),
              ],
            );
          }).toList(),
    );
  }
}
