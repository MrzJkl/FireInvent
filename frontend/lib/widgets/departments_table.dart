import 'package:flameguardlaundry/models/department_model.dart';
import 'package:flutter/material.dart';
import 'package:data_table_2/data_table_2.dart';

class DepartmentsTable extends StatelessWidget {
  final List<DepartmentModel> departments;
  final void Function(DepartmentModel) onEdit;
  final void Function(String) onDelete;

  const DepartmentsTable({
    super.key,
    required this.departments,
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
        DataColumn(label: Text('Description')),
        DataColumn(label: Text('Actions')),
      ],
      rows:
          departments.map((department) {
            return DataRow(
              cells: [
                DataCell(Text(department.name)),
                DataCell(Text(department.description ?? '')),
                DataCell(
                  Row(
                    children: [
                      IconButton(
                        icon: const Icon(Icons.edit),
                        onPressed: () => onEdit(department),
                        tooltip: 'Edit',
                      ),
                      IconButton(
                        icon: const Icon(Icons.delete),
                        onPressed: () => onDelete(department.id),
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
