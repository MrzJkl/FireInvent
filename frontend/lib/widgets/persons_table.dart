import 'package:flameguardlaundry/models/person_model.dart';
import 'package:flutter/material.dart';
import 'package:data_table_2/data_table_2.dart';

class PersonsTable extends StatelessWidget {
  final List<PersonModel> persons;
  final void Function(PersonModel) onEdit;
  final void Function(String) onDelete;

  const PersonsTable({
    super.key,
    required this.persons,
    required this.onEdit,
    required this.onDelete,
  });

  @override
  Widget build(BuildContext context) {
    return DataTable2(
      columnSpacing: 16,
      horizontalMargin: 12,
      columns: const [
        DataColumn(label: Text('Firstname')),
        DataColumn(label: Text('Lastname')),
        DataColumn(label: Text('Remarks')),
        DataColumn(label: Text('Contact Info')),
        DataColumn(label: Text('External ID')),
        DataColumn(label: Text('Actions')),
      ],
      rows:
          persons.map((person) {
            return DataRow(
              cells: [
                DataCell(Text(person.firstName)),
                DataCell(Text(person.lastName)),
                DataCell(Text(person.remarks ?? '')),
                DataCell(Text(person.contactInfo ?? '')),
                DataCell(Text(person.externalId ?? '')),
                DataCell(
                  Row(
                    children: [
                      IconButton(
                        icon: const Icon(Icons.edit),
                        onPressed: () => onEdit(person),
                        tooltip: 'Edit',
                      ),
                      IconButton(
                        icon: const Icon(Icons.delete),
                        onPressed: () => onDelete(person.id),
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
