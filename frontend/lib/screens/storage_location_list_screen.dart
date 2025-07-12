import 'package:fireinvent/constants.dart';
import 'package:fireinvent/main_drawer.dart';
import 'package:fireinvent/models/create_models/create_storage_location_model.dart';
import 'package:fireinvent/models/storage_location_model.dart';
import 'package:fireinvent/services/storage_location_service.dart';
import 'package:fireinvent/widgets/storage_location_form.dart';
import 'package:fireinvent/widgets/storage_locations_table.dart';
import 'package:flutter/material.dart';
import 'package:get_it/get_it.dart';

class StorageLocationListScreen extends StatefulWidget {
  const StorageLocationListScreen({super.key});

  @override
  State<StorageLocationListScreen> createState() =>
      _StorageLocationListScreenState();
}

class _StorageLocationListScreenState extends State<StorageLocationListScreen> {
  late Future<List<StorageLocationModel>> _storageLocationsFuture;
  final StorageLocationService _service = GetIt.I<StorageLocationService>();

  @override
  void initState() {
    super.initState();
    _loadData();
  }

  void _loadData() {
    setState(() {
      _storageLocationsFuture = _service.getAll();
    });
  }

  void _delete(String id) async {
    await _service.delete(id);
    _loadData();
  }

  Future<void> showStorageLocationFormDialog({
    required BuildContext context,
    StorageLocationModel? initial,
    required void Function(String? id, CreateStorageLocationModel) onSubmit,
  }) async {
    await showDialog(
      context: context,
      builder:
          (_) => AlertDialog(
            title: Text(
              initial == null
                  ? 'Create Storage Location'
                  : 'Edit Storage Location',
            ),
            content: SizedBox(
              width: 400,
              child: StorageLocationForm(
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

  void _edit(StorageLocationModel model) async {
    await showStorageLocationFormDialog(
      context: context,
      initial: model,
      onSubmit: (id, updated) async {
        final updatedModel = StorageLocationModel(
          id: id!,
          name: updated.name,
          remarks: updated.remarks,
        );
        await _service.update(id, updatedModel);
        _loadData();
      },
    );
  }

  void _create() async {
    await showStorageLocationFormDialog(
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
        tooltip: 'Add Storage Location',
        child: const Icon(Icons.add),
      ),
      body: FutureBuilder<List<StorageLocationModel>>(
        future: _storageLocationsFuture,
        builder: (context, snapshot) {
          if (snapshot.connectionState == ConnectionState.waiting) {
            return const Center(child: CircularProgressIndicator());
          } else if (snapshot.hasError) {
            return Center(child: Text('Error: ${snapshot.error}'));
          } else if (!snapshot.hasData || snapshot.data!.isEmpty) {
            return const Center(child: Text('No storage locations available.'));
          }

          final storageLocations = snapshot.data!;

          return StorageLocationsTable(
            storageLocations: storageLocations,
            onEdit: _edit,
            onDelete: _delete,
          );
        },
      ),
    );
  }
}
