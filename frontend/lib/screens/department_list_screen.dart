import 'package:fireinvent/constants.dart';
import 'package:fireinvent/main_drawer.dart';
import 'package:fireinvent/models/create_models/create_department_model.dart';
import 'package:fireinvent/models/department_model.dart';
import 'package:fireinvent/services/department_service.dart';
import 'package:fireinvent/widgets/department_form.dart';
import 'package:fireinvent/widgets/departments_table.dart';
import 'package:flutter/material.dart';
import 'package:get_it/get_it.dart';

class DepartmentListScreen extends StatefulWidget {
  const DepartmentListScreen({super.key});

  @override
  State<DepartmentListScreen> createState() => _DepartmentListScreenState();
}

class _DepartmentListScreenState extends State<DepartmentListScreen> {
  late Future<List<DepartmentModel>> _departmentsFuture;
  final DepartmentService _service = GetIt.I<DepartmentService>();

  @override
  void initState() {
    super.initState();
    _loadData();
  }

  void _loadData() {
    setState(() {
      _departmentsFuture = _service.getAll();
    });
  }

  void _delete(String id) async {
    await _service.delete(id);
    _loadData();
  }

  Future<void> showDepartmentFormDialog({
    required BuildContext context,
    DepartmentModel? initial,
    required void Function(String? id, CreateDepartmentModel) onSubmit,
  }) async {
    await showDialog(
      context: context,
      builder:
          (_) => AlertDialog(
            title: Text(
              initial == null ? 'Create Department' : 'Edit Department',
            ),
            content: SizedBox(
              width: 400,
              child: DepartmentForm(
                initial: initial,
                onSubmit: (createModel) {
                  Navigator.of(context).pop();
                  onSubmit(initial?.id, createModel);
                },
              ),
            ),
          ),
    );
  }

  void _edit(DepartmentModel model) async {
    await showDepartmentFormDialog(
      context: context,
      initial: model,
      onSubmit: (id, updated) async {
        final updatedModel = DepartmentModel(
          id: id!,
          name: updated.name,
          description: updated.description,
        );
        await _service.update(id, updatedModel);
        _loadData();
      },
    );
  }

  void _create() async {
    await showDepartmentFormDialog(
      context: context,
      onSubmit: (id, created) async {
        await _service.create(created);
        _loadData();
      },
    );
  }

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      appBar: Constants.buildMainAppBar(context),
      drawer: MainDrawer(),
      floatingActionButton: FloatingActionButton(
        onPressed: _create,
        tooltip: 'Add Department',
        child: const Icon(Icons.add),
      ),
      body: FutureBuilder<List<DepartmentModel>>(
        future: _departmentsFuture,
        builder: (context, snapshot) {
          if (snapshot.connectionState == ConnectionState.waiting) {
            return const Center(child: CircularProgressIndicator());
          } else if (snapshot.hasError) {
            return Center(child: Text('Error: ${snapshot.error}'));
          } else if (!snapshot.hasData || snapshot.data!.isEmpty) {
            return const Center(child: Text('No departments available.'));
          }

          final departments = snapshot.data!;

          return DepartmentsTable(
            departments: departments,
            onEdit: _edit,
            onDelete: _delete,
          );
        },
      ),
    );
  }
}
