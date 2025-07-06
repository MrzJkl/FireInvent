import 'package:flameguardlaundry/models/storage_location_model.dart';
import 'package:flutter/material.dart';
import 'package:data_table_2/data_table_2.dart';

class StorageLocationsTable extends StatelessWidget {
  final List<StorageLocationModel> storageLocations;
  final void Function(StorageLocationModel) onEdit;
  final void Function(String) onDelete;

  const StorageLocationsTable({
    super.key,
    required this.storageLocations,
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
        DataColumn(label: Text('Remarks')),
        DataColumn(label: Text('Actions')),
      ],
      rows:
          storageLocations.map((storageLocation) {
            return DataRow(
              cells: [
                DataCell(Text(storageLocation.name)),
                DataCell(Text(storageLocation.remarks ?? '')),
                DataCell(
                  Row(
                    children: [
                      IconButton(
                        icon: const Icon(Icons.edit),
                        onPressed: () => onEdit(storageLocation),
                        tooltip: 'Edit',
                      ),
                      IconButton(
                        icon: const Icon(Icons.delete),
                        onPressed: () => onDelete(storageLocation.id),
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
